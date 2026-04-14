using GameJobNotifier.App.Models;
using GameJobNotifier.App.Services.Interfaces;

namespace GameJobNotifier.App.Services;

public sealed class SyncEventHub : ISyncEventHub
{
    public event EventHandler<SyncResult>? SyncCompleted;

    public void Publish(SyncResult result)
    {
        SyncCompleted?.Invoke(this, result);
    }
}
