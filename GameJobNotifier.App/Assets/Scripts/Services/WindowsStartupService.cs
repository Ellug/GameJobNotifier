using GameJobNotifier.App.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace GameJobNotifier.App.Services;

public sealed class WindowsStartupService(
    ILogger<WindowsStartupService> logger) : IWindowsStartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "GameJobNotifier";

    private readonly ILogger<WindowsStartupService> _logger = logger;

    public bool TryConfigure(bool enabled, out string? errorMessage)
    {
        errorMessage = null;

        try
        {
            using var runKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true) ??
                               Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);

            if (runKey is null)
            {
                errorMessage = "Windows 시작프로그램 레지스트리 키를 열 수 없습니다.";
                return false;
            }

            if (enabled)
            {
                var startupCommand = BuildStartupCommand();
                if (string.IsNullOrWhiteSpace(startupCommand))
                {
                    errorMessage = "실행 파일 경로를 확인할 수 없습니다.";
                    return false;
                }

                runKey.SetValue(ValueName, startupCommand, RegistryValueKind.String);
            }
            else
            {
                runKey.DeleteValue(ValueName, throwOnMissingValue: false);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to configure Windows startup.");
            errorMessage = ex.Message;
            return false;
        }
    }

    private static string? BuildStartupCommand()
    {
        var processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath))
        {
            return null;
        }

        return $"\"{processPath}\"";
    }
}
