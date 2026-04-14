namespace GameJobNotifier.App.Services.Interfaces;

public interface ITrayIconService : IDisposable
{
    event Action? OpenRequested;

    event Action? CheckRequested;

    event Action? ExitRequested;

    void Initialize();

    void ShowBalloon(string title, string message, string? clickUrl = null);
}
