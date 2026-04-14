namespace GameJobNotifier.App.Models;

public sealed record SyncResult
{
    public required DateTimeOffset StartedAtUtc { get; init; }

    public required DateTimeOffset CompletedAtUtc { get; init; }

    public int FetchedCount { get; init; }

    public int MatchedCount { get; init; }

    public string CollectorName { get; init; } = string.Empty;

    public IReadOnlyList<JobChange> Changes { get; init; } = [];

    public string? ErrorMessage { get; init; }

    public bool IsSuccess => string.IsNullOrWhiteSpace(ErrorMessage);

    public int NewCount => Changes.Count(change => change.ChangeType == JobChangeType.Added);

    public int HiddenCount => Changes.Count(change => change.ChangeType == JobChangeType.Hidden);

    public int RestoredCount => Changes.Count(change => change.ChangeType == JobChangeType.Restored);
}
