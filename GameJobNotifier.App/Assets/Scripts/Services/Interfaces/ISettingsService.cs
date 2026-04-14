using GameJobNotifier.App.Models;

namespace GameJobNotifier.App.Services.Interfaces;

public interface ISettingsService
{
    Task<AppSettings> GetAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
