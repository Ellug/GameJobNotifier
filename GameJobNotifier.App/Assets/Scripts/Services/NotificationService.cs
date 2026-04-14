using System.Diagnostics;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Text.Json;
using GameJobNotifier.App.Models;
using GameJobNotifier.App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GameJobNotifier.App.Services;

public sealed class NotificationService(
    ITrayIconService trayIconService,
    ILogger<NotificationService> logger) : INotificationService
{
    private const string NewPostingTitle = "신규 공고";
    private const string DiscordWebhookUrl =
        "https://discord.com/api/webhooks/1493502216135381104/gGjX1eEQ0aBTgKGx1Vk-1AHFm3UhQztjw3cKIP98dYCrZW9c3zQdNt-yktVjznQdjRnE";

    private static readonly HttpClient DiscordHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private readonly ITrayIconService _trayIconService = trayIconService;
    private readonly ILogger<NotificationService> _logger = logger;

    public async Task NotifyPostingAsync(
        JobPosting posting,
        AppSettings settings,
        string notificationTitle,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (settings.EnableToastNotification)
        {
            TryShowToastViaPowerShell(posting, notificationTitle);
        }

        if (settings.EnableTrayBalloon)
        {
            _trayIconService.ShowBalloon(
                notificationTitle,
                $"{posting.Company} | {posting.Title}",
                posting.DetailUrl);
        }

        if (settings.EnableDiscordWebhook &&
            string.Equals(notificationTitle, NewPostingTitle, StringComparison.Ordinal))
        {
            await TrySendDiscordWebhookAsync(posting, notificationTitle, cancellationToken);
        }
    }

    private void TryShowToastViaPowerShell(JobPosting posting, string notificationTitle)
    {
        try
        {
            var title = EscapeXml($"GameJobNotifier {notificationTitle}");
            var line1 = EscapeXml(posting.Title);
            var line2 = EscapeXml($"{posting.Company} | {posting.CareerText} | {posting.LocationText}");
            var openableUrl = NormalizeHttpUrl(posting.DetailUrl);
            var actions = string.Empty;
            if (openableUrl is not null)
            {
                var escapedUrl = EscapeXml(openableUrl);
                actions = $$"""
                  <actions>
                    <action content="공고 열기" activationType="protocol" arguments="{{escapedUrl}}"/>
                  </actions>
                """;
            }

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
                {{actions}}
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

    private async Task TrySendDiscordWebhookAsync(
        JobPosting posting,
        string notificationTitle,
        CancellationToken cancellationToken)
    {
        try
        {
            var detailUrl = NormalizeHttpUrl(posting.DetailUrl) ?? posting.DetailUrl;
            var content = BuildDiscordContent(posting, notificationTitle, detailUrl);
            var payload = JsonSerializer.Serialize(new
            {
                username = "GameJobNotifier",
                content
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, DiscordWebhookUrl)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            using var response = await DiscordHttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Discord webhook failed. status={StatusCode}",
                    (int)response.StatusCode);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Discord webhook notification failed.");
        }
    }

    private static string BuildDiscordContent(JobPosting posting, string notificationTitle, string detailUrl)
    {
        var builder = new StringBuilder();
        builder.Append('[').Append(notificationTitle).Append("] ").Append(posting.Title).AppendLine();
        builder.Append("회사: ").Append(posting.Company).AppendLine();
        builder.Append("경력: ").Append(posting.CareerText).AppendLine();
        builder.Append("지역: ").Append(posting.LocationText).AppendLine();
        builder.Append("링크: ").Append(detailUrl);
        return builder.ToString();
    }
}
