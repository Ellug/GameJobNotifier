using GameJobNotifier.App.Models;
using System.Text.RegularExpressions;

namespace GameJobNotifier.App.Services;

public static partial class JobFilter
{
    public static bool MatchesPrimaryCriteria(JobPosting posting, JobFilterCriteria criteria)
    {
        if (!MatchesDuty(posting.DutyText, criteria.DutyNames))
        {
            return false;
        }

        if (!MatchesByKeyword(posting.LocationText, criteria.RegionKeywords))
        {
            return false;
        }

        if (!MatchesByKeyword(posting.GameCategoryText, criteria.GameFieldKeywords))
        {
            return false;
        }

        if (!MatchesByKeyword(posting.EmploymentTypeText, criteria.WorkConditionKeywords))
        {
            return false;
        }

        return MatchesCareer(posting.CareerText, criteria.QualificationKeywords);
    }

    private static bool MatchesDuty(string dutyText, IReadOnlySet<string> selectedDutyNames)
    {
        if (selectedDutyNames.Count == 0 || string.IsNullOrWhiteSpace(dutyText))
        {
            return false;
        }

        var duties = dutyText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeDutyName);

        return duties.Any(selectedDutyNames.Contains);
    }

    private static bool MatchesByKeyword(string source, IReadOnlySet<string> keywords)
    {
        if (keywords.Count == 0)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            return false;
        }

        return keywords.Any(keyword => source.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesCareer(string careerText, IReadOnlySet<string> qualificationKeywords)
    {
        if (qualificationKeywords.Count == 0)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(careerText))
        {
            return false;
        }

        foreach (var keyword in qualificationKeywords)
        {
            if (keyword.Equals("신입", StringComparison.Ordinal) &&
                careerText.Contains("신입", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (keyword.Equals("경력무관", StringComparison.Ordinal) &&
                careerText.Contains("경력무관", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (keyword.Equals("1~3년", StringComparison.Ordinal) && MatchesCareerRange(careerText, 1, 3))
            {
                return true;
            }

            if (keyword.Equals("3~5년", StringComparison.Ordinal) && MatchesCareerRange(careerText, 3, 5))
            {
                return true;
            }

            if (keyword.Equals("5년↑", StringComparison.Ordinal) && MatchesCareerRange(careerText, 5, int.MaxValue))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesCareerRange(string text, int targetMin, int targetMax)
    {
        if (TryExtractCareerRange(text, out var min, out var max))
        {
            return min <= targetMax && max >= targetMin;
        }

        if (TryExtractAtLeastYears(text, out var atLeast))
        {
            return atLeast <= targetMax && int.MaxValue >= targetMin;
        }

        if (TryExtractSingleYear(text, out var single))
        {
            return single >= targetMin && single <= targetMax;
        }

        return false;
    }

    private static bool TryExtractCareerRange(string text, out int min, out int max)
    {
        min = 0;
        max = 0;

        var match = CareerRangeRegex().Match(text);
        if (!match.Success)
        {
            return false;
        }

        if (!int.TryParse(match.Groups["from"].Value, out min) ||
            !int.TryParse(match.Groups["to"].Value, out max))
        {
            return false;
        }

        if (max < min)
        {
            (min, max) = (max, min);
        }

        return true;
    }

    private static bool TryExtractAtLeastYears(string text, out int min)
    {
        min = 0;

        var match = CareerAtLeastRegex().Match(text);
        if (!match.Success)
        {
            return false;
        }

        return int.TryParse(match.Groups["from"].Value, out min);
    }

    private static bool TryExtractSingleYear(string text, out int year)
    {
        year = 0;

        var match = SingleCareerYearRegex().Match(text);
        if (!match.Success)
        {
            return false;
        }

        return int.TryParse(match.Groups["year"].Value, out year);
    }

    private static string NormalizeDutyName(string raw)
    {
        return raw.Trim();
    }

    [GeneratedRegex(@"(?<from>\d+)\s*[~\-]\s*(?<to>\d+)\s*년", RegexOptions.CultureInvariant)]
    private static partial Regex CareerRangeRegex();

    [GeneratedRegex(@"(?<from>\d+)\s*년\s*(?:이상|↑)", RegexOptions.CultureInvariant)]
    private static partial Regex CareerAtLeastRegex();

    [GeneratedRegex(@"(?<year>\d+)\s*년", RegexOptions.CultureInvariant)]
    private static partial Regex SingleCareerYearRegex();
}
