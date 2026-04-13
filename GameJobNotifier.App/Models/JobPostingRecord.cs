namespace GameJobNotifier.App.Models;

public sealed record JobPostingRecord
{
    public required string JobId { get; init; }

    public required string SourceUrl { get; init; }

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

    public required string ModifiedKey { get; init; }

    public bool IsHidden { get; init; }

    public DateTimeOffset FirstSeenUtc { get; init; }

    public DateTimeOffset LastSeenUtc { get; init; }

    public DateTimeOffset LastChangedUtc { get; init; }

    public JobPosting ToPosting()
    {
        return new JobPosting
        {
            JobId = JobId,
            DetailUrl = DetailUrl,
            Title = Title,
            Company = Company,
            DutyText = DutyText,
            CareerText = CareerText,
            EducationText = EducationText,
            LocationText = LocationText,
            GameCategoryText = GameCategoryText,
            EmploymentTypeText = EmploymentTypeText,
            DeadlineText = DeadlineText,
            ModifiedText = ModifiedText,
            ModifiedKey = ModifiedKey
        };
    }
}
