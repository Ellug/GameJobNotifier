namespace GameJobNotifier.App.Models;

public sealed record FilterSelectionSnapshot
{
    public required IReadOnlyList<int> DutyCodes { get; init; }

    public required IReadOnlyList<string> Regions { get; init; }

    public required IReadOnlyList<string> GameFields { get; init; }

    public required IReadOnlyList<string> WorkConditions { get; init; }

    public required IReadOnlyList<string> Qualifications { get; init; }
}
