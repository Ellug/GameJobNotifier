namespace GameJobNotifier.App.Models;

public static class FilterOptionCatalog
{
    public static IReadOnlyList<string> RegionOptions { get; } =
    [
        "서울",
        "경기",
        "인천",
        "부산",
        "대구",
        "광주",
        "대전",
        "울산",
        "세종",
        "강원",
        "충북",
        "충남",
        "전북",
        "전남",
        "경북",
        "경남",
        "제주",
        "전국",
        "재택"
    ];

    public static IReadOnlyList<string> GameFieldOptions { get; } =
    [
        "온라인PC게임",
        "모바일게임",
        "콘솔게임",
        "멀티플랫폼게임",
        "웹게임",
        "RPG",
        "Casual",
        "Shooting",
        "SNG",
        "Simulation",
        "Arcade",
        "VR·AR"
    ];

    public static IReadOnlyList<string> WorkConditionOptions { get; } =
    [
        "정규직",
        "계약직",
        "인턴",
        "아르바이트",
        "프리랜서",
        "병역특례",
        "교육생"
    ];

    public static IReadOnlyList<string> QualificationOptions { get; } =
    [
        "신입",
        "경력무관",
        "1~3년",
        "3~5년",
        "5년↑"
    ];

    public static IReadOnlyList<string> DefaultQualificationOptions { get; } =
    [
        "신입",
        "경력무관",
        "1~3년"
    ];

    private static readonly IReadOnlySet<string> RegionOptionSet = RegionOptions.ToHashSet(StringComparer.Ordinal);
    private static readonly IReadOnlySet<string> GameFieldOptionSet = GameFieldOptions.ToHashSet(StringComparer.Ordinal);
    private static readonly IReadOnlySet<string> WorkConditionOptionSet = WorkConditionOptions.ToHashSet(StringComparer.Ordinal);
    private static readonly IReadOnlySet<string> QualificationOptionSet = QualificationOptions.ToHashSet(StringComparer.Ordinal);

    public static IReadOnlyList<string> NormalizeRegions(IEnumerable<string>? selected)
    {
        return Normalize(selected, RegionOptionSet);
    }

    public static IReadOnlyList<string> NormalizeGameFields(IEnumerable<string>? selected)
    {
        return Normalize(selected, GameFieldOptionSet);
    }

    public static IReadOnlyList<string> NormalizeWorkConditions(IEnumerable<string>? selected)
    {
        return Normalize(selected, WorkConditionOptionSet);
    }

    public static IReadOnlyList<string> NormalizeQualifications(IEnumerable<string>? selected)
    {
        var normalized = Normalize(selected, QualificationOptionSet);
        return normalized.Count == 0 ? DefaultQualificationOptions : normalized;
    }

    private static IReadOnlyList<string> Normalize(IEnumerable<string>? selected, IReadOnlySet<string> allowed)
    {
        return (selected ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Where(allowed.Contains)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
    }
}
