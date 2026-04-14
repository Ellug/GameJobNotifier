using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using GameJobNotifier.App.Services.Interfaces;

namespace GameJobNotifier.App.Services;

public sealed class TrayIconService : ITrayIconService
{
    private NotifyIcon? _notifyIcon;
    private ContextMenuStrip? _menu;
    private Icon? _trayIcon;
    private string? _pendingBalloonUrl;

    public event Action? OpenRequested;
    public event Action? CheckRequested;
    public event Action? ExitRequested;

    public void Initialize()
    {
        if (_notifyIcon is not null)
        {
            return;
        }

        _menu = new ContextMenuStrip();
        _menu.Items.Add("열기", null, (_, _) => OpenRequested?.Invoke());
        _menu.Items.Add("지금 검사", null, (_, _) => CheckRequested?.Invoke());
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add("종료", null, (_, _) => ExitRequested?.Invoke());

        _trayIcon = ResolveTrayIcon();
        _notifyIcon = new NotifyIcon
        {
            Icon = _trayIcon,
            Text = "GameJobNotifier",
            Visible = true,
            ContextMenuStrip = _menu
        };

        _notifyIcon.DoubleClick += OnNotifyIconDoubleClick;
        _notifyIcon.BalloonTipClicked += OnBalloonTipClicked;
    }

    public void ShowBalloon(string title, string message, string? clickUrl = null)
    {
        _pendingBalloonUrl = NormalizeHttpUrl(clickUrl);
        _notifyIcon?.ShowBalloonTip(5000, title, message, ToolTipIcon.Info);
    }

    public void Dispose()
    {
        if (_notifyIcon is not null)
        {
            _notifyIcon.DoubleClick -= OnNotifyIconDoubleClick;
            _notifyIcon.BalloonTipClicked -= OnBalloonTipClicked;
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        _pendingBalloonUrl = null;
        _trayIcon?.Dispose();
        _trayIcon = null;

        _menu?.Dispose();
        _menu = null;
    }

    private static Icon ResolveTrayIcon()
    {
        try
        {
            var processPath = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(processPath))
            {
                var extracted = Icon.ExtractAssociatedIcon(processPath);
                if (extracted is not null)
                {
                    return (Icon)extracted.Clone();
                }
            }
        }
        catch
        {
            // Use default icon when extraction fails.
        }

        return (Icon)SystemIcons.Information.Clone();
    }

    private void OnNotifyIconDoubleClick(object? sender, EventArgs e)
    {
        OpenRequested?.Invoke();
    }

    private void OnBalloonTipClicked(object? sender, EventArgs e)
    {
        var url = _pendingBalloonUrl;
        _pendingBalloonUrl = null;

        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            // Ignore URL launch failures.
        }
    }

    private static string? NormalizeHttpUrl(string? rawUrl)
    {
        if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return null;
        }

        return uri.AbsoluteUri;
    }
}
