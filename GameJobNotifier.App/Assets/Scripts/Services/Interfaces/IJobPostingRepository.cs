using GameJobNotifier.App.Models;

namespace GameJobNotifier.App.Services.Interfaces;

public interface IJobPostingRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, JobPostingRecord>> GetByIdsAsync(
        string sourceUrl,
        IReadOnlyCollection<string> jobIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetVisibleJobIdsAsync(
        string sourceUrl,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        IReadOnlyCollection<JobPostingRecord> records,
        CancellationToken cancellationToken = default);

    Task MarkHiddenAsync(
        string sourceUrl,
        IReadOnlyCollection<string> jobIds,
        DateTimeOffset hiddenAtUtc,
        CancellationToken cancellationToken = default);
}
