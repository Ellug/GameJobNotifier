using GameJobNotifier.App.Models;

namespace GameJobNotifier.App.Services.Interfaces;

public interface ISyncEventHub
{
    event EventHandler<SyncResult>? SyncCompleted;

    void Publish(SyncResult result);
}
