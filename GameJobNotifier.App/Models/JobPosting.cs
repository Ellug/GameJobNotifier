namespace GameJobNotifier.App.Models;

public sealed record JobPosting
{
    public required string JobId { get; init; }

    public required string DetailUrl { get; init; }

    public required string Title { get; init; }

    public required string Company { get; init; }

    public string DutyText { get; init; } = string.Empty;

    public string CareerText { get; init; } = string.Empty;

    public string EducationText { get; init; } = string.Empty;

    public string LocationText { get; init; } = string.Empty;

    public string GameCategoryText { get; init; } = string.Empty;

    public string EmploymentTypeText { get; init; } = string.Empty;

    public string DeadlineText { get; init; } = string.Empty;

    public string ModifiedText { get; init; } = string.Empty;

    public string ModifiedKey { get; init; } = string.Empty;
}
