using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace GameJobNotifier.App.Infrastructure;

public static class WindowsAppIdentity
{
    public const string AppUserModelId = "GameJobNotifier.App";

    public static void EnsureExplicitAppUserModelId(ILogger? logger = null)
    {
        try
        {
            var hr = SetCurrentProcessExplicitAppUserModelID(AppUserModelId);
            if (hr < 0)
            {
                logger?.LogWarning(
                    "Failed to set AppUserModelID. hr=0x{Hr}",
                    hr.ToString("X8"));
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to set AppUserModelID.");
        }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(string appID);
}
