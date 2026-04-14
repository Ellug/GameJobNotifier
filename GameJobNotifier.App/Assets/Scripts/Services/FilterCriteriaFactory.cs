using GameJobNotifier.App.Models;
using GameJobNotifier.App.Services.Interfaces;

namespace GameJobNotifier.App.Services;

public sealed class FilterCriteriaFactory : IFilterCriteriaFactory
{
    public JobFilterCriteria Create(AppSettings settings)
    {
        return new JobFilterCriteria
        {
            DutyNames = DutyCatalog.ResolveDutyNames(settings.SelectedDutyCodes),
            RegionKeywords = settings.SelectedRegions.ToHashSet(StringComparer.Ordinal),
            GameFieldKeywords = settings.SelectedGameFields.ToHashSet(StringComparer.Ordinal),
            WorkConditionKeywords = settings.SelectedWorkConditions.ToHashSet(StringComparer.Ordinal),
            QualificationKeywords = settings.SelectedQualifications.ToHashSet(StringComparer.Ordinal)
        };
    }
}
