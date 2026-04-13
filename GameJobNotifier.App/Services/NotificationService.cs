using System.Diagnostics;
using System.Security;
using System.Text;
using GameJobNotifier.App.Models;
using GameJobNotifier.App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GameJobNotifier.App.Services;

public sealed class NotificationService(
    ITrayIconService trayIconService,
    ILogger<NotificationService> logger) : INotificationService
{
    private readonly ITrayIconService _trayIconService = trayIconService;
    private readonly ILogger<NotificationService> _logger = logger;

    public Task NotifyNewPostingAsync(
        JobPosting posting,
        AppSettings settings,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (settings.EnableToastNotification)
        {
            TryShowToastViaPowerShell(posting);
        }

        if (settings.EnableTrayBalloon)
        {
            _trayIconService.ShowBalloon("신규 공고", $"{posting.Company} | {posting.Title}");
        }

        return Task.CompletedTask;
    }

    private void TryShowToastViaPowerShell(JobPosting posting)
    {
        try
        {
            var title = EscapeXml("GameJobNotifier 신규 공고");
            var line1 = EscapeXml(posting.Title);
            var line2 = EscapeXml($"{posting.Company} | {posting.CareerText} | {posting.LocationText}");

            var script = $$"""
                $ErrorActionPreference = 'Stop'
                [Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] > $null
                [Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] > $null

                $xml = @"
                <toast scenario="reminder" duration="long">
                  <visual>
                    <binding template="ToastGeneric">
                      <text>{{title}}</text>
                      <text>{{line1}}</text>
                      <text>{{line2}}</text>
                    </binding>
                  </visual>
                  <audio silent="true"/>
                </toast>
                "@

                $doc = New-Object Windows.Data.Xml.Dom.XmlDocument
                $doc.LoadXml($xml)
                $toast = [Windows.UI.Notifications.ToastNotification]::new($doc)
                [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('GameJobNotifier').Show($toast)
                """;

            var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));

            _ = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -WindowStyle Hidden -EncodedCommand {encoded}",
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Toast notification failed.");
        }
    }

    private static string EscapeXml(string value)
    {
        return SecurityElement.Escape(value) ?? string.Empty;
    }
}
