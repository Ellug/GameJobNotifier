using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameJobNotifier.App.Infrastructure;
using GameJobNotifier.App.Models;
using GameJobNotifier.App.Services.Interfaces;
using GameJobNotifier.App.ViewModels.Containers;
using Microsoft.Extensions.Logging;

namespace GameJobNotifier.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IRuntimeStateService _runtimeStateService;
    private readonly ICheckRequestQueue _checkRequestQueue;
    private readonly ISyncEventHub _syncEventHub;
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty]
    private int _checkIntervalMinutes = 10;

    [ObservableProperty]
    private bool _enableToastNotification = true;

    [ObservableProperty]
    private bool _enableTrayBalloon = true;

    [ObservableProperty]
    private bool _startInBackground;

    [ObservableProperty]
    private string _statusText = "대기 중";

    [ObservableProperty]
    private string _lastRunText = "최근 검사: 없음";

    [ObservableProperty]
    private bool _isSaving;

    public ObservableCollection<ActivityLogEntry> ActivityLogs { get; } = [];

    public DutyFilterContainerViewModel DutyFilter { get; } = new();

    public MultiSelectFilterContainerViewModel RegionFilter { get; } =
        new("선택 지역", "전체", FilterOptionCatalog.RegionOptions);

    public MultiSelectFilterContainerViewModel GameFieldFilter { get; } =
        new("선택 게임분야", "전체", FilterOptionCatalog.GameFieldOptions);

    public MultiSelectFilterContainerViewModel WorkConditionFilter { get; } =
        new("선택 근무조건", "전체", FilterOptionCatalog.WorkConditionOptions);

    public MultiSelectFilterContainerViewModel QualificationFilter { get; } =
        new("선택 지원자격", "전체", FilterOptionCatalog.QualificationOptions);

    public MainViewModel(
        ISettingsService settingsService,
        IRuntimeStateService runtimeStateService,
        ICheckRequestQueue checkRequestQueue,
        ISyncEventHub syncEventHub,
        ILogger<MainViewModel> logger)
    {
        _settingsService = settingsService;
        _runtimeStateService = runtimeStateService;
        _checkRequestQueue = checkRequestQueue;
        _syncEventHub = syncEventHub;
        _logger = logger;

        _syncEventHub.SyncCompleted += HandleSyncCompleted;
        _ = LoadSettingsAsync();
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        var snapshot = BuildSelectionSnapshot();
        if (snapshot.DutyCodes.Count == 0)
        {
            StatusText = "저장 실패: 직종을 1개 이상 선택하세요.";
            AppendLog("설정 저장 실패 - 직종 미선택");
            return;
        }

        try
        {
            IsSaving = true;

            var settings = new AppSettings
            {
                CheckIntervalMinutes = CheckIntervalMinutes,
                EnableToastNotification = EnableToastNotification,
                EnableTrayBalloon = EnableTrayBalloon,
                StartInBackground = StartInBackground,
                SelectedDutyCodes = snapshot.DutyCodes,
                SelectedRegions = snapshot.Regions,
                SelectedGameFields = snapshot.GameFields,
                SelectedWorkConditions = snapshot.WorkConditions,
                SelectedQualifications = snapshot.Qualifications
            }.Sanitize();

            await _settingsService.SaveAsync(settings);
            ApplySettings(settings);

            StatusText = "설정 저장 완료";
            AppendLog(
                $"설정을 저장했습니다. (직종 {settings.SelectedDutyCodes.Count}, 지역 {settings.SelectedRegions.Count}, 게임분야 {settings.SelectedGameFields.Count}, 근무조건 {settings.SelectedWorkConditions.Count}, 지원자격 {settings.SelectedQualifications.Count})");

            _checkRequestQueue.RequestCheck();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings.");
            StatusText = $"저장 실패: {ex.Message}";
            AppendLog($"설정 저장 실패 - {ex.Message}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void RunNow()
    {
        _checkRequestQueue.RequestCheck();
        AppendLog("수동 검사 요청됨");
        StatusText = "수동 검사 요청됨";
    }

    [RelayCommand]
    private void OpenDataFolder()
    {
        AppPaths.EnsureCreated();

        Process.Start(new ProcessStartInfo
        {
            FileName = AppPaths.BaseDirectory,
            UseShellExecute = true
        });
    }

    [RelayCommand]
    private void OpenLogItem(ActivityLogEntry? entry)
    {
        if (entry is null || !entry.HasUrl)
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = entry.Url!,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open log item URL.");
            AppendLog($"로그 링크 열기 실패 - {ex.Message}");
        }
    }

    private FilterSelectionSnapshot BuildSelectionSnapshot()
    {
        return new FilterSelectionSnapshot
        {
            DutyCodes = DutyFilter.GetSelectedCodes(),
            Regions = RegionFilter.GetSelectedKeys(),
            GameFields = GameFieldFilter.GetSelectedKeys(),
            WorkConditions = WorkConditionFilter.GetSelectedKeys(),
            Qualifications = QualificationFilter.GetSelectedKeys()
        };
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            var settings = await _settingsService.GetAsync();
            var runtimeState = await _runtimeStateService.GetAsync();
            ApplySettings(settings);

            if (runtimeState.LastSuccessfulCheckUtc is not null)
            {
                LastRunText = $"최근 검사: {runtimeState.LastSuccessfulCheckUtc.Value.LocalDateTime:yyyy-MM-dd HH:mm:ss}";
            }

            AppendLog("설정을 불러왔습니다.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings.");
            AppendLog($"설정 로드 실패 - {ex.Message}");
        }
    }

    private void ApplySettings(AppSettings settings)
    {
        void Apply()
        {
            CheckIntervalMinutes = settings.CheckIntervalMinutes;
            EnableToastNotification = settings.EnableToastNotification;
            EnableTrayBalloon = settings.EnableTrayBalloon;
            StartInBackground = settings.StartInBackground;

            DutyFilter.ApplySelection(settings.SelectedDutyCodes);
            RegionFilter.ApplySelection(settings.SelectedRegions);
            GameFieldFilter.ApplySelection(settings.SelectedGameFields);
            WorkConditionFilter.ApplySelection(settings.SelectedWorkConditions);
            QualificationFilter.ApplySelection(settings.SelectedQualifications);
        }

        if (System.Windows.Application.Current.Dispatcher.CheckAccess())
        {
            Apply();
            return;
        }

        System.Windows.Application.Current.Dispatcher.Invoke(Apply);
    }

    private void HandleSyncCompleted(object? sender, SyncResult result)
    {
        void Apply()
        {
            LastRunText = $"최근 검사: {result.CompletedAtUtc.LocalDateTime:yyyy-MM-dd HH:mm:ss}";

            if (!result.IsSuccess)
            {
                StatusText = $"오류: {result.ErrorMessage}";
                AppendLog($"검사 실패 - {result.ErrorMessage}");
                return;
            }

            StatusText =
                $"완료 | 수집 {result.FetchedCount}건, 조건일치 {result.MatchedCount}건, 신규 {result.NewCount}건";
            AppendLog(
                $"검사 완료 ({result.CollectorName}) - 신규 {result.NewCount}, 수정 {result.UpdatedCount}, 삭제 {result.HiddenCount}, 복원 {result.RestoredCount}");

            AppendChangeDetails(result.Changes);
        }

        if (System.Windows.Application.Current.Dispatcher.CheckAccess())
        {
            Apply();
            return;
        }

        System.Windows.Application.Current.Dispatcher.Invoke(Apply);
    }

    private void AppendChangeDetails(IReadOnlyList<JobChange> changes)
    {
        if (changes.Count == 0)
        {
            AppendLog("변경 공고 없음");
            return;
        }

        const int maxDetailLogs = 40;
        foreach (var change in changes.Take(maxDetailLogs))
        {
            var label = change.ChangeType switch
            {
                JobChangeType.Added => "신규",
                JobChangeType.Updated => "수정",
                JobChangeType.Hidden => "비노출",
                JobChangeType.Restored => "복원",
                _ => "변경"
            };

            var posting = change.Posting;
            AppendLog($"{label} | [{posting.JobId}] {posting.Company} | {posting.Title}", posting.DetailUrl);
        }

        if (changes.Count > maxDetailLogs)
        {
            AppendLog($"변경 상세 로그 생략 {changes.Count - maxDetailLogs}건");
        }
    }

    private void AppendLog(string message, string? url = null)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        ActivityLogs.Insert(0, new ActivityLogEntry
        {
            Text = line,
            Url = url
        });

        const int maxLogCount = 250;
        while (ActivityLogs.Count > maxLogCount)
        {
            ActivityLogs.RemoveAt(ActivityLogs.Count - 1);
        }
    }
}
