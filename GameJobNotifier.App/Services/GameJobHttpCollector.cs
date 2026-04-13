using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using GameJobNotifier.App.Models;
using GameJobNotifier.App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GameJobNotifier.App.Services;

public sealed partial class GameJobHttpCollector(
    HttpClient httpClient,
    IGameJobHtmlParser htmlParser,
    ILogger<GameJobHttpCollector> logger) : IJobCollector
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IGameJobHtmlParser _htmlParser = htmlParser;
    private readonly ILogger<GameJobHttpCollector> _logger = logger;

    public async Task<JobCollectionResult> CollectAsync(Uri targetUrl, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, targetUrl);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var rawBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var html = DecodeHtml(response.Content.Headers, rawBytes);
        var postings = _htmlParser.Parse(html, targetUrl);

        _logger.LogInformation("HTTP collector fetched {Count} postings from {Url}", postings.Count, targetUrl);
        return new JobCollectionResult(postings, "HttpClient");
    }

    private static string DecodeHtml(HttpContentHeaders headers, byte[] payload)
    {
        var charset = headers.ContentType?.CharSet?.Trim('"');
        var encoding = ResolveEncoding(charset) ?? ResolveEncoding(DetectMetaCharset(payload)) ?? Encoding.UTF8;
        return encoding.GetString(payload);
    }

    private static Encoding? ResolveEncoding(string? charset)
    {
        if (string.IsNullOrWhiteSpace(charset))
        {
            return null;
        }

        try
        {
            return Encoding.GetEncoding(charset);
        }
        catch
        {
            return null;
        }
    }

    private static string? DetectMetaCharset(byte[] payload)
    {
        var sample = Encoding.ASCII.GetString(payload, 0, Math.Min(payload.Length, 2048));
        var match = MetaCharsetRegex().Match(sample);
        return match.Success ? match.Groups["charset"].Value : null;
    }

    [GeneratedRegex(@"charset=(?<charset>[A-Za-z0-9\-_]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex MetaCharsetRegex();
}
