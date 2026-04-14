namespace GameJobNotifier.App.Services.Interfaces;

public interface IWindowsStartupService
{
    bool TryConfigure(bool enabled, out string? errorMessage);
}
