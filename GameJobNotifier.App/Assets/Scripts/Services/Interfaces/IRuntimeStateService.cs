using GameJobNotifier.App.Models;

namespace GameJobNotifier.App.Services.Interfaces;

public interface IRuntimeStateService
{
    Task<RuntimeState> GetAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(RuntimeState state, CancellationToken cancellationToken = default);
}
