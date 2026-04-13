namespace GameJobNotifier.App.Models;

public sealed record DutyCategoryOption
{
    public required int Code { get; init; }

    public required int GroupCode { get; init; }

    public required string Name { get; init; }

    public int PostingCount { get; init; }
}
