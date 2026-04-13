namespace GameJobNotifier.App.Models;

public sealed record JobFilterCriteria
{
    public required IReadOnlySet<string> DutyNames { get; init; }

    public required IReadOnlySet<string> RegionKeywords { get; init; }

    public required IReadOnlySet<string> GameFieldKeywords { get; init; }

    public required IReadOnlySet<string> WorkConditionKeywords { get; init; }

    public required IReadOnlySet<string> QualificationKeywords { get; init; }
}
