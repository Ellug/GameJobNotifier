using GameJobNotifier.App.Models;

namespace GameJobNotifier.App.Services.Interfaces;

public interface IFilterCriteriaFactory
{
    JobFilterCriteria Create(AppSettings settings);
}
