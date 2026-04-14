using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using GameJobNotifier.App.Models;
using GameJobNotifier.App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GameJobNotifier.App.Services;

public sealed class JobSyncService(
    IJobCollector collector,
    ISettingsService settingsService,
    IRuntimeStateService runtimeStateService,
    IJobPostingRepository repository,
    IFilterCriteriaFactory filterCriteriaFactory,
    INotificationService notificationService,
    ISyncEventHub syncEventHub,
    TimeProvider timeProvider,
    ILogger<JobSyncService> logger) : IJobSyncService
{
    private readonly IJobCollector _collector = collector;
    private readonly ISettingsService _settingsService = settingsService;
    private readonly IRuntimeStateService _runtimeStateService = runtimeStateService;
    private readonly IJobPostingRepository _repository = repository;
    private readonly IFilterCriteriaFactory _filterCriteriaFactory = filterCriteriaFactory;
    private readonly INotificationService _notificationService = notificationService;
    private readonly ISyncEventHub _syncEventHub = syncEventHub;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<JobSyncService> _logger = logger;

    private static readonly Regex MinutesAgoRegex = new(@"(?<value>\d+)\s*분\s*전\s*등록", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private static readonly Regex HoursAgoRegex = new(@"(?<value>\d+)\s*시간\s*전\s*등록", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private static readonly Regex DaysAgoRegex = new(@"(?<value>\d+)\s*일\s*전\s*등록", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private static readonly Regex MonthDayRegex = new(@"(?<month>\d{1,2})\s*/\s*(?<day>\d{1,2})\s*(?:\([^)]+\))?\s*등록", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public async Task<SyncResult> SyncAsync(CancellationToken cancellationToken = default)
    {
        var startedAt = _timeProvider.GetUtcNow();

        try
        {
            var settings = (await _settingsService.GetAsync(cancellationToken)).Sanitize();
            var sourceScopeKey = BuildSourceScopeKey(settings);
            var runtimeState = await _runtimeStateService.GetAsync(cancellationToken);
            var previousSuccessfulCheckUtc = runtimeState.LastSuccessfulCheckUtc;
            var syncReferenceNowUtc = _timeProvider.GetUtcNow();
            var criteria = _filterCriteriaFactory.Create(settings);

            if (!Uri.TryCreate(settings.TargetUrl, UriKind.Absolute, out var targetUri))
            {
                throw new InvalidOperationException($"Invalid target URL: {settings.TargetUrl}");
            }

            await _repository.InitializeAsync(cancellationToken);

            var collection = await _collector.CollectAsync(targetUri, cancellationToken);
            var matchedPostings = collection.Postings
                .Where(posting => JobFilter.MatchesPrimaryCriteria(posting, criteria))
                .DistinctBy(posting => posting.JobId)
                .ToList();

            var now = _timeProvider.GetUtcNow();
            var existingById = await _repository.GetByIdsAsync(
                sourceScopeKey,
                matchedPostings.Select(posting => posting.JobId).ToArray(),
                cancellationToken);

            var upserts = new List<JobPostingRecord>(matchedPostings.Count);
            var changes = new List<JobChange>();

            foreach (var posting in matchedPostings)
            {
                if (!existingById.TryGetValue(posting.JobId, out var existing))
                {
                    upserts.Add(CreateRecord(sourceScopeKey, posting, now));
                    changes.Add(new JobChange(JobChangeType.Added, posting));
                    continue;
                }

                var updatedRecord = CreateRecord(sourceScopeKey, posting, now, existing.FirstSeenUtc);
                upserts.Add(updatedRecord);

                if (existing.IsHidden)
                {
                    changes.Add(new JobChange(JobChangeType.Restored, posting));
                }
            }

            if (upserts.Count > 0)
            {
                await _repository.UpsertAsync(upserts, cancellationToken);
            }

            var currentIds = matchedPostings.Select(posting => posting.JobId).ToHashSet(StringComparer.Ordinal);
            var visibleIds = await _repository.GetVisibleJobIdsAsync(sourceScopeKey, cancellationToken);
            var missingIds = visibleIds.Where(id => !currentIds.Contains(id)).ToArray();

            if (missingIds.Length > 0)
            {
                var hiddenRecords = await _repository.GetByIdsAsync(
                    sourceScopeKey,
                    missingIds,
                    cancellationToken);
                await _repository.MarkHiddenAsync(sourceScopeKey, missingIds, now, cancellationToken);

                foreach (var hiddenId in missingIds)
                {
                    if (hiddenRecords.TryGetValue(hiddenId, out var hiddenRecord))
                    {
                        changes.Add(new JobChange(JobChangeType.Hidden, hiddenRecord.ToPosting()));
                    }
                }
            }

            if (settings.EnableToastNotification || settings.EnableTrayBalloon)
            {
                foreach (var change in changes)
                {
                    string? notificationTitle = change.ChangeType switch
                    {
                        JobChangeType.Added => "신규 공고",
                        JobChangeType.Restored => "복원 공고",
                        _ => null
                    };

                    if (notificationTitle is null)
                    {
                        continue;
                    }

                    if (change.ChangeType == JobChangeType.Added &&
                        !ShouldNotifyForPeriod(change.Posting, previousSuccessfulCheckUtc, syncReferenceNowUtc))
                    {
                        continue;
                    }

                    await _notificationService.NotifyPostingAsync(
                        change.Posting,
                        settings,
                        notificationTitle,
                        cancellationToken);
                }
            }

            var completedAtUtc = _timeProvider.GetUtcNow();
            var result = new SyncResult
            {
                StartedAtUtc = startedAt,
                CompletedAtUtc = completedAtUtc,
                FetchedCount = collection.Postings.Count,
                MatchedCount = matchedPostings.Count,
                CollectorName = collection.CollectorName,
                Changes = changes
            };

            try
            {
                await _runtimeStateService.SaveAsync(runtimeState with
                {
                    LastCheckAttemptUtc = completedAtUtc,
                    LastSuccessfulCheckUtc = completedAtUtc
                }, cancellationToken);
            }
            catch (Exception stateEx)
            {
                _logger.LogWarning(stateEx, "Failed to persist runtime state after sync success.");
            }

            _syncEventHub.Publish(result);
            _logger.LogInformation(
                "Sync finished. fetched={Fetched}, matched={Matched}, new={NewCount}, hidden={HiddenCount}, restored={RestoredCount}",
                result.FetchedCount,
                result.MatchedCount,
                result.NewCount,
                result.HiddenCount,
                result.RestoredCount);

            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync failed.");

            var completedAtUtc = _timeProvider.GetUtcNow();
            try
            {
                var runtimeState = await _runtimeStateService.GetAsync(cancellationToken);
                await _runtimeStateService.SaveAsync(runtimeState with
                {
                    LastCheckAttemptUtc = completedAtUtc
                }, cancellationToken);
            }
            catch (Exception stateEx)
            {
                _logger.LogWarning(stateEx, "Failed to persist runtime state after sync failure.");
            }

            var failed = new SyncResult
            {
                StartedAtUtc = startedAt,
                CompletedAtUtc = completedAtUtc,
                ErrorMessage = ex.Message
            };

            _syncEventHub.Publish(failed);
            return failed;
        }
    }

    private static JobPostingRecord CreateRecord(
        string sourceUrl,
        JobPosting posting,
        DateTimeOffset now,
        DateTimeOffset? firstSeenUtc = null)
    {
        return new JobPostingRecord
        {
            JobId = posting.JobId,
            SourceUrl = sourceUrl,
            DetailUrl = posting.DetailUrl,
            Title = posting.Title,
            Company = posting.Company,
            DutyText = posting.DutyText,
            CareerText = posting.CareerText,
            EducationText = posting.EducationText,
            LocationText = posting.LocationText,
            GameCategoryText = posting.GameCategoryText,
            EmploymentTypeText = posting.EmploymentTypeText,
            DeadlineText = posting.DeadlineText,
            RegisteredText = posting.RegisteredText,
            IsHidden = false,
            FirstSeenUtc = firstSeenUtc ?? now,
            LastSeenUtc = now,
            LastChangedUtc = now
        };
    }

    private static bool ShouldNotifyForPeriod(
        JobPosting posting,
        DateTimeOffset? previousSuccessfulCheckUtc,
        DateTimeOffset nowUtc)
    {
        // First run has no baseline. Avoid notifying the entire current list.
        if (previousSuccessfulCheckUtc is null)
        {
            return false;
        }

        if (!TryResolveRegisteredAtUtc(posting.RegisteredText, nowUtc, out var registeredAtUtc))
        {
            // If registration text is not parseable, rely on added-change signal.
            return true;
        }

        return registeredAtUtc > previousSuccessfulCheckUtc.Value && registeredAtUtc <= nowUtc;
    }

    private static bool TryResolveRegisteredAtUtc(string modifiedText, DateTimeOffset nowUtc, out DateTimeOffset registeredAtUtc)
    {
        registeredAtUtc = default;
        if (string.IsNullOrWhiteSpace(modifiedText))
        {
            return false;
        }

        var trimmed = modifiedText.Trim();

        if (TryParseRelative(trimmed, MinutesAgoRegex, minutes => nowUtc.AddMinutes(-minutes), out registeredAtUtc) ||
            TryParseRelative(trimmed, HoursAgoRegex, hours => nowUtc.AddHours(-hours), out registeredAtUtc) ||
            TryParseRelative(trimmed, DaysAgoRegex, days => nowUtc.AddDays(-days), out registeredAtUtc))
        {
            return true;
        }

        var dateMatch = MonthDayRegex.Match(trimmed);
        if (!dateMatch.Success)
        {
            return false;
        }

        var localNow = nowUtc.ToLocalTime();
        if (!int.TryParse(dateMatch.Groups["month"].Value, out var month) ||
            !int.TryParse(dateMatch.Groups["day"].Value, out var day))
        {
            return false;
        }

        var year = localNow.Year;
        DateTimeOffset localDate;
        try
        {
            localDate = new DateTimeOffset(year, month, day, 0, 0, 0, localNow.Offset);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }

        if (localDate > localNow.AddDays(1))
        {
            localDate = localDate.AddYears(-1);
        }

        registeredAtUtc = localDate.ToUniversalTime();
        return true;
    }

    private static bool TryParseRelative(
        string text,
        Regex regex,
        Func<int, DateTimeOffset> convert,
        out DateTimeOffset result)
    {
        result = default;

        var match = regex.Match(text);
        if (!match.Success)
        {
            return false;
        }

        if (!int.TryParse(match.Groups["value"].Value, out var value))
        {
            return false;
        }

        result = convert(value);
        return true;
    }

    private static string BuildSourceScopeKey(AppSettings settings)
    {
        var payload = string.Join("|",
            settings.TargetUrl,
            string.Join(",", settings.SelectedDutyCodes.Select(code => code.ToString(CultureInfo.InvariantCulture))),
            string.Join(",", settings.SelectedRegions),
            string.Join(",", settings.SelectedGameFields),
            string.Join(",", settings.SelectedWorkConditions),
            string.Join(",", settings.SelectedQualifications));

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return $"scope:{Convert.ToHexString(hash)}";
    }
}
