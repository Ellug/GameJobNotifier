namespace GameJobNotifier.App.Services.Interfaces;

public interface ICheckRequestQueue
{
    void RequestCheck();

    ValueTask WaitForCheckAsync(CancellationToken cancellationToken = default);

    bool TryDequeue();
}
