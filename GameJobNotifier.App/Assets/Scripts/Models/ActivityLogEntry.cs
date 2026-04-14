namespace GameJobNotifier.App.Models;

public sealed record ActivityLogEntry
{
    public required string Text { get; init; }

    public string? Url { get; init; }

    public bool HasUrl => !string.IsNullOrWhiteSpace(Url);
}
