using System.IO;

namespace GameJobNotifier.App.Infrastructure;

public static class AppPaths
{
    public static string BaseDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GameJobNotifier");

    public static string SettingsFile => Path.Combine(BaseDirectory, "settings.json");

    public static string RuntimeStateFile => Path.Combine(BaseDirectory, "runtime-state.json");

    public static string DatabaseFile => Path.Combine(BaseDirectory, "gamejob-notifier.sqlite3");

    public static void EnsureCreated()
    {
        Directory.CreateDirectory(BaseDirectory);
    }
}
