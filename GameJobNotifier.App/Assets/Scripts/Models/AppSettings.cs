namespace GameJobNotifier.App.Models;

public sealed record AppSettings
{
    public static readonly string DefaultTargetUrl = DutyCatalog.BuildDutyUrl(DutyCatalog.DefaultSelectedCodes);

    public string TargetUrl { get; init; } = DefaultTargetUrl;

    public int CheckIntervalMinutes { get; init; } = 10;

    public bool EnableToastNotification { get; init; } = true;

    public bool EnableTrayBalloon { get; init; } = true;

    public bool EnableDiscordWebhook { get; init; }

    public bool StartInBackground { get; init; }

    public IReadOnlyList<int> SelectedDutyCodes { get; init; } = DutyCatalog.DefaultSelectedCodes;

    public IReadOnlyList<string> SelectedRegions { get; init; } = [];

    public IReadOnlyList<string> SelectedGameFields { get; init; } = [];

    public IReadOnlyList<string> SelectedWorkConditions { get; init; } = [];

    public IReadOnlyList<string> SelectedQualifications { get; init; } = FilterOptionCatalog.DefaultQualificationOptions;

    public AppSettings Sanitize()
    {
        var normalizedInterval = Math.Clamp(CheckIntervalMinutes, 1, 1440);
        var normalizedDutyCodes = DutyCatalog.NormalizeSelectedCodes(SelectedDutyCodes);
        var normalizedUrl = DutyCatalog.BuildDutyUrl(normalizedDutyCodes);
        var normalizedRegions = FilterOptionCatalog.NormalizeRegions(SelectedRegions);
        var normalizedGameFields = FilterOptionCatalog.NormalizeGameFields(SelectedGameFields);
        var normalizedWorkConditions = FilterOptionCatalog.NormalizeWorkConditions(SelectedWorkConditions);
        var normalizedQualifications = FilterOptionCatalog.NormalizeQualifications(SelectedQualifications);

        return this with
        {
            TargetUrl = normalizedUrl,
            CheckIntervalMinutes = normalizedInterval,
            SelectedDutyCodes = normalizedDutyCodes,
            SelectedRegions = normalizedRegions,
            SelectedGameFields = normalizedGameFields,
            SelectedWorkConditions = normalizedWorkConditions,
            SelectedQualifications = normalizedQualifications
        };
    }
}
