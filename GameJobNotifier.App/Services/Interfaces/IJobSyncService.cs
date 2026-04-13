using GameJobNotifier.App.Models;

namespace GameJobNotifier.App.Services.Interfaces;

public interface IJobSyncService
{
    Task<SyncResult> SyncAsync(CancellationToken cancellationToken = default);
}
