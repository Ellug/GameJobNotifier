using System.Net;
using System.Text.RegularExpressions;
using GameJobNotifier.App.Models;
using GameJobNotifier.App.Services.Interfaces;
using HtmlAgilityPack;

namespace GameJobNotifier.App.Services;

public sealed partial class GameJobHtmlParser : IGameJobHtmlParser
{
    public IReadOnlyList<JobPosting> Parse(string html, Uri baseUri)
    {
        var document = new HtmlAgilityPack.HtmlDocument();
        document.LoadHtml(html);

        var rows = document.DocumentNode.SelectNodes("//article[contains(@class,'boothList')]//table[contains(@class,'tblList')]//tbody/tr")
                   ?? document.DocumentNode.SelectNodes("//table[contains(@class,'tblList')]//tbody/tr");

        if (rows is null)
        {
            return [];
        }

        var postings = new List<JobPosting>();

        foreach (var row in rows)
        {
            var anchor = row.SelectSingleNode(".//div[contains(@class,'tit')]//a[contains(@href,'/Recruit/GI_Read/View')]");
            if (anchor is null)
            {
                continue;
            }

            var href = anchor.GetAttributeValue("href", string.Empty).Trim();
            var jobId = ExtractJobId(href);
            if (string.IsNullOrWhiteSpace(jobId))
            {
                continue;
            }

            var title = Clean(anchor.InnerText);
            var company = Clean(row.SelectSingleNode(".//div[contains(@class,'company')]//strong")?.InnerText);
            var infoValues = row.SelectNodes(".//p[contains(@class,'info')]/span")
                                 ?.Select(span => Clean(span.InnerText))
                                 .Where(text => !string.IsNullOrWhiteSpace(text))
                                 .ToArray()
                             ?? [];

            var career = infoValues.ElementAtOrDefault(0) ?? string.Empty;
            var education = infoValues.ElementAtOrDefault(1) ?? string.Empty;
            var location = infoValues.ElementAtOrDefault(2) ?? string.Empty;
            var gameCategory = infoValues.ElementAtOrDefault(3) ?? string.Empty;
            var employmentType = infoValues.ElementAtOrDefault(4) ?? string.Empty;

            var deadline = Clean(row.SelectSingleNode(".//span[contains(@class,'date')]")?.InnerText);
            var modified = Clean(row.SelectSingleNode(".//span[contains(@class,'modifyDate')]")?.InnerText);

            var (dutyFromOnclick, gameFromOnclick, locationFromOnclick) =
                ParseOnclickMeta(anchor.GetAttributeValue("onclick", string.Empty));

            postings.Add(new JobPosting
            {
                JobId = jobId,
                DetailUrl = BuildAbsoluteUrl(baseUri, href),
                Title = title,
                Company = company,
                DutyText = dutyFromOnclick,
                CareerText = career,
                EducationText = education,
                LocationText = string.IsNullOrWhiteSpace(location) ? locationFromOnclick : location,
                GameCategoryText = string.IsNullOrWhiteSpace(gameCategory) ? gameFromOnclick : gameCategory,
                EmploymentTypeText = employmentType,
                DeadlineText = deadline,
                RegisteredText = modified
            });
        }

        return postings;
    }

    private static string ExtractJobId(string href)
    {
        var match = JobIdRegex().Match(href);
        return match.Success ? match.Groups["id"].Value : string.Empty;
    }

    private static string BuildAbsoluteUrl(Uri baseUri, string href)
    {
        if (string.IsNullOrWhiteSpace(href))
        {
            return string.Empty;
        }

        if (Uri.TryCreate(href, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        return new Uri(baseUri, href).ToString();
    }

    private static (string Duty, string GameCategory, string Location) ParseOnclickMeta(string onclick)
    {
        if (string.IsNullOrWhiteSpace(onclick))
        {
            return (string.Empty, string.Empty, string.Empty);
        }

        var decoded = WebUtility.HtmlDecode(onclick);
        var matches = OnclickMetaRegex().Matches(decoded);
        if (matches.Count == 0)
        {
            return (string.Empty, string.Empty, string.Empty);
        }

        var duty = matches.Count >= 1 ? Clean(matches[0].Groups["value"].Value) : string.Empty;
        var game = matches.Count >= 2 ? Clean(matches[1].Groups["value"].Value) : string.Empty;
        var location = matches.Count >= 3 ? Clean(matches[2].Groups["value"].Value) : string.Empty;
        return (duty, game, location);
    }

    private static string Clean(string? rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return string.Empty;
        }

        var decoded = WebUtility.HtmlDecode(rawText).Replace('\u00A0', ' ');
        return MultiWhitespaceRegex().Replace(decoded, " ").Trim();
    }

    [GeneratedRegex(@"GI_No=(?<id>\d+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex JobIdRegex();

    [GeneratedRegex(@"IsNullOrWhiteSpace\('(?<value>[^']*)'\)", RegexOptions.CultureInvariant)]
    private static partial Regex OnclickMetaRegex();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex MultiWhitespaceRegex();
}
