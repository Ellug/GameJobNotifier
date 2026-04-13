namespace GameJobNotifier.App.Models;

public sealed record RuntimeState
{
    public DateTimeOffset? LastCheckAttemptUtc { get; init; }

    public DateTimeOffset? LastSuccessfulCheckUtc { get; init; }
}
