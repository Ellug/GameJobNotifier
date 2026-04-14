using System.Net.Http;
using System.Net.Http.Headers;
using System.Globalization;
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
        var collected = new List<JobPosting>();
        var pagesFetched = 0;

        var firstPageHtml = await FetchHtmlByGetAsync(targetUrl, cancellationToken);
        pagesFetched++;
        collected.AddRange(_htmlParser.Parse(firstPageHtml, targetUrl));

        var maxPage = ExtractMaxPageNumber(firstPageHtml);
        if (maxPage > 1)
        {
            for (var page = 2; page <= maxPage; page++)
            {
                var pagedHtml = await TryFetchHtmlByPostAsync(targetUrl, page, cancellationToken);
                if (string.IsNullOrWhiteSpace(pagedHtml))
                {
                    continue;
                }

                pagesFetched++;
                collected.AddRange(_htmlParser.Parse(pagedHtml, targetUrl));
            }
        }

        var postings = collected
            .GroupBy(posting => posting.JobId, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();

        _logger.LogInformation(
            "HTTP collector fetched {Count} postings from {Url} (pages={Pages})",
            postings.Length,
            targetUrl,
            pagesFetched);

        return new JobCollectionResult(postings, "HttpClient");
    }

    private async Task<string> FetchHtmlByGetAsync(Uri targetUrl, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, targetUrl);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var rawBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return DecodeHtml(response.Content.Headers, rawBytes);
    }

    private async Task<string?> TryFetchHtmlByPostAsync(Uri targetUrl, int page, CancellationToken cancellationToken)
    {
        var payload = BuildPagedRequestPayload(targetUrl, page);
        if (payload.Count == 0)
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(targetUrl, "/Recruit/_GI_Job_List/"));
        request.Content = new FormUrlEncodedContent(payload);
        request.Headers.Referrer = targetUrl;
        request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Failed to fetch paged list from GameJob. page={Page}, status={StatusCode}, url={Url}",
                page,
                response.StatusCode,
                targetUrl);
            return null;
        }

        var rawBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return DecodeHtml(response.Content.Headers, rawBytes);
    }

    private static int ExtractMaxPageNumber(string html)
    {
        var matches = PaginationPageRegex().Matches(html);
        if (matches.Count == 0)
        {
            return 1;
        }

        var max = 1;
        foreach (Match match in matches)
        {
            if (!int.TryParse(match.Groups["page"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var page))
            {
                continue;
            }

            if (page > max)
            {
                max = page;
            }
        }

        return max;
    }

    private static IReadOnlyList<KeyValuePair<string, string>> BuildPagedRequestPayload(Uri targetUrl, int page)
    {
        var query = ParseQuery(targetUrl.Query);
        if (!query.TryGetValue("menucode", out var menuCode) ||
            !menuCode.Equals("duty", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        if (!query.TryGetValue("duty", out var dutyRaw))
        {
            return [];
        }

        var dutyCodes = dutyRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (dutyCodes.Length == 0)
        {
            return [];
        }

        var payload = new List<KeyValuePair<string, string>>
        {
            new("page", page.ToString(CultureInfo.InvariantCulture)),
            new("pagesize", "40"),
            new("order", "1"),
            new("tabcode", "1"),
            new("direct", "0"),
            new("condition[menucode]", "duty"),
            new("condition[searchtype]", "B"),
            new("condition[duty]", string.Join(",", dutyCodes))
        };

        foreach (var dutyCode in dutyCodes)
        {
            payload.Add(new KeyValuePair<string, string>("condition[dutyArr][]", dutyCode));
            payload.Add(new KeyValuePair<string, string>("condition[dutySelect][]", dutyCode));
        }

        return payload;
    }

    private static Dictionary<string, string> ParseQuery(string queryString)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(queryString))
        {
            return result;
        }

        var trimmed = queryString.TrimStart('?');
        foreach (var pair in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=');
            if (separator < 0)
            {
                continue;
            }

            var key = Uri.UnescapeDataString(pair[..separator]).Trim();
            var value = Uri.UnescapeDataString(pair[(separator + 1)..]).Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            result[key] = value;
        }

        return result;
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

    [GeneratedRegex(@"data-page=""(?<page>\d+)""", RegexOptions.CultureInvariant)]
    private static partial Regex PaginationPageRegex();
}
