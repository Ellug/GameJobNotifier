namespace GameJobNotifier.App.Models;

public sealed record DutyCategoryGroup
{
    public required int Code { get; init; }

    public required string Name { get; init; }

    public required IReadOnlyList<DutyCategoryOption> Options { get; init; }
}
