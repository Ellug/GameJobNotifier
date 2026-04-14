namespace GameJobNotifier.App.Models;

public static class DutyCatalog
{
    public const int ClientDutyCode = 1;

    private static readonly IReadOnlyList<DutyCategoryGroup> GroupItems =
    [
        new DutyCategoryGroup
        {
            Code = 1,
            Name = "게임제작",
            Options =
            [
                new DutyCategoryOption
                {
                    Code = 1,
                    GroupCode = 1,
                    Name = "게임개발(클라이언트)",
                },
                new DutyCategoryOption
                {
                    Code = 2,
                    GroupCode = 1,
                    Name = "게임개발(모바일)",
                },
                new DutyCategoryOption
                {
                    Code = 3,
                    GroupCode = 1,
                    Name = "게임AI 개발",
                },
                new DutyCategoryOption
                {
                    Code = 4,
                    GroupCode = 1,
                    Name = "인터페이스 디자인",
                },
                new DutyCategoryOption
                {
                    Code = 5,
                    GroupCode = 1,
                    Name = "원화",
                },
                new DutyCategoryOption
                {
                    Code = 6,
                    GroupCode = 1,
                    Name = "모델링",
                },
                new DutyCategoryOption
                {
                    Code = 7,
                    GroupCode = 1,
                    Name = "애니메이션",
                },
                new DutyCategoryOption
                {
                    Code = 8,
                    GroupCode = 1,
                    Name = "이펙트·FX",
                },
                new DutyCategoryOption
                {
                    Code = 9,
                    GroupCode = 1,
                    Name = "게임기획",
                },
            ]
        },
        new DutyCategoryGroup
        {
            Code = 2,
            Name = "사운드·영상제작",
            Options =
            [
                new DutyCategoryOption
                {
                    Code = 10,
                    GroupCode = 2,
                    Name = "사운드 제작",
                },
                new DutyCategoryOption
                {
                    Code = 11,
                    GroupCode = 2,
                    Name = "영상제작·편집",
                },
            ]
        },
        new DutyCategoryGroup
        {
            Code = 3,
            Name = "플랫폼 제작",
            Options =
            [
                new DutyCategoryOption
                {
                    Code = 12,
                    GroupCode = 3,
                    Name = "플랫폼 개발",
                },
                new DutyCategoryOption
                {
                    Code = 13,
                    GroupCode = 3,
                    Name = "플랫폼 디자인",
                },
                new DutyCategoryOption
                {
                    Code = 14,
                    GroupCode = 3,
                    Name = "BX·브랜드 디자인",
                },
                new DutyCategoryOption
                {
                    Code = 15,
                    GroupCode = 3,
                    Name = "플랫폼 기획",
                },
            ]
        },
        new DutyCategoryGroup
        {
            Code = 4,
            Name = "기술지원",
            Options =
            [
                new DutyCategoryOption
                {
                    Code = 16,
                    GroupCode = 4,
                    Name = "서버",
                },
                new DutyCategoryOption
                {
                    Code = 17,
                    GroupCode = 4,
                    Name = "네트워크",
                },
                new DutyCategoryOption
                {
                    Code = 18,
                    GroupCode = 4,
                    Name = "엔진",
                },
                new DutyCategoryOption
                {
                    Code = 19,
                    GroupCode = 4,
                    Name = "시스템·DB",
                },
                new DutyCategoryOption
                {
                    Code = 20,
                    GroupCode = 4,
                    Name = "보안",
                },
                new DutyCategoryOption
                {
                    Code = 21,
                    GroupCode = 4,
                    Name = "클라우드",
                },
                new DutyCategoryOption
                {
                    Code = 22,
                    GroupCode = 4,
                    Name = "결제·PG",
                },
            ]
        },
        new DutyCategoryGroup
        {
            Code = 5,
            Name = "게임운영·QA",
            Options =
            [
                new DutyCategoryOption
                {
                    Code = 23,
                    GroupCode = 5,
                    Name = "게임운영",
                },
                new DutyCategoryOption
                {
                    Code = 24,
                    GroupCode = 5,
                    Name = "QA·테스터",
                },
            ]
        },
        new DutyCategoryGroup
        {
            Code = 6,
            Name = "사업기획",
            Options =
            [
                new DutyCategoryOption
                {
                    Code = 25,
                    GroupCode = 6,
                    Name = "사업기획(국내)",
                },
                new DutyCategoryOption
                {
                    Code = 26,
                    GroupCode = 6,
                    Name = "사업기획(해외)",
                },
                new DutyCategoryOption
                {
                    Code = 27,
                    GroupCode = 6,
                    Name = "전략기획",
                },
                new DutyCategoryOption
                {
                    Code = 28,
                    GroupCode = 6,
                    Name = "사업관리",
                },
                new DutyCategoryOption
                {
                    Code = 29,
                    GroupCode = 6,
                    Name = "경영분석",
                },
                new DutyCategoryOption
                {
                    Code = 30,
                    GroupCode = 6,
                    Name = "정보·빅데이터 분석",
                },
                new DutyCategoryOption
                {
                    Code = 31,
                    GroupCode = 6,
                    Name = "리서치",
                },
                new DutyCategoryOption
                {
                    Code = 32,
                    GroupCode = 6,
                    Name = "임원·CEO",
                },
            ]
        },
        new DutyCategoryGroup
        {
            Code = 7,
            Name = "마케팅·미디어",
            Options =
            [
                new DutyCategoryOption
                {
                    Code = 33,
                    GroupCode = 7,
                    Name = "마케팅",
                },
                new DutyCategoryOption
                {
                    Code = 34,
                    GroupCode = 7,
                    Name = "홍보",
                },
                new DutyCategoryOption
                {
                    Code = 35,
                    GroupCode = 7,
                    Name = "해외 마케팅",
                },
                new DutyCategoryOption
                {
                    Code = 36,
                    GroupCode = 7,
                    Name = "프로모션",
                },
                new DutyCategoryOption
                {
                    Code = 37,
                    GroupCode = 7,
                    Name = "광고기획",
                },
                new DutyCategoryOption
                {
                    Code = 38,
                    GroupCode = 7,
                    Name = "취재",
                },
                new DutyCategoryOption
                {
                    Code = 39,
                    GroupCode = 7,
                    Name = "게임기자",
                },
                new DutyCategoryOption
                {
                    Code = 40,
                    GroupCode = 7,
                    Name = "투자배급",
                },
                new DutyCategoryOption
                {
                    Code = 41,
                    GroupCode = 7,
                    Name = "CRM",
                },
                new DutyCategoryOption
                {
                    Code = 42,
                    GroupCode = 7,
                    Name = "제휴",
                },
                new DutyCategoryOption
                {
                    Code = 43,
                    GroupCode = 7,
                    Name = "프로게이머",
                },
                new DutyCategoryOption
                {
                    Code = 44,
                    GroupCode = 7,
                    Name = "E-Sports",
                },
                new DutyCategoryOption
                {
                    Code = 45,
                    GroupCode = 7,
                    Name = "방송",
                },
                new DutyCategoryOption
                {
                    Code = 46,
                    GroupCode = 7,
                    Name = "퍼블리싱",
                },
                new DutyCategoryOption
                {
                    Code = 47,
                    GroupCode = 7,
                    Name = "소싱/라이선싱",
                },
                new DutyCategoryOption
                {
                    Code = 48,
                    GroupCode = 7,
                    Name = "채널링",
                },
            ]
        },
        new DutyCategoryGroup
        {
            Code = 8,
            Name = "영업·영업관리",
            Options =
            [
                new DutyCategoryOption
                {
                    Code = 49,
                    GroupCode = 8,
                    Name = "국내영업",
                },
                new DutyCategoryOption
                {
                    Code = 50,
                    GroupCode = 8,
                    Name = "해외영업",
                },
                new DutyCategoryOption
                {
                    Code = 51,
                    GroupCode = 8,
                    Name = "영업 관리",
                },
                new DutyCategoryOption
                {
                    Code = 52,
                    GroupCode = 8,
                    Name = "해외지사 관리",
                },
                new DutyCategoryOption
                {
                    Code = 53,
                    GroupCode = 8,
                    Name = "기술영업",
                },
                new DutyCategoryOption
                {
                    Code = 54,
                    GroupCode = 8,
                    Name = "PC방 영업·관리",
                },
            ]
        },
        new DutyCategoryGroup
        {
            Code = 9,
            Name = "경영지원",
            Options =
            [
                new DutyCategoryOption
                {
                    Code = 55,
                    GroupCode = 9,
                    Name = "사무·회계",
                },
                new DutyCategoryOption
                {
                    Code = 56,
                    GroupCode = 9,
                    Name = "재무",
                },
                new DutyCategoryOption
                {
                    Code = 57,
                    GroupCode = 9,
                    Name = "회계",
                },
                new DutyCategoryOption
                {
                    Code = 58,
                    GroupCode = 9,
                    Name = "경리",
                },
                new DutyCategoryOption
                {
                    Code = 59,
                    GroupCode = 9,
                    Name = "세무",
                },
                new DutyCategoryOption
                {
                    Code = 60,
                    GroupCode = 9,
                    Name = "통계",
                },
                new DutyCategoryOption
                {
                    Code = 61,
                    GroupCode = 9,
                    Name = "주식IR",
                },
                new DutyCategoryOption
                {
                    Code = 62,
                    GroupCode = 9,
                    Name = "CFO",
                },
                new DutyCategoryOption
                {
                    Code = 63,
                    GroupCode = 9,
                    Name = "CFA",
                },
                new DutyCategoryOption
                {
                    Code = 64,
                    GroupCode = 9,
                    Name = "인사",
                },
                new DutyCategoryOption
                {
                    Code = 65,
                    GroupCode = 9,
                    Name = "법무",
                },
                new DutyCategoryOption
                {
                    Code = 66,
                    GroupCode = 9,
                    Name = "총무",
                },
                new DutyCategoryOption
                {
                    Code = 67,
                    GroupCode = 9,
                    Name = "구매",
                },
            ]
        },
        new DutyCategoryGroup
        {
            Code = 10,
            Name = "교육·강의",
            Options =
            [
                new DutyCategoryOption
                {
                    Code = 68,
                    GroupCode = 10,
                    Name = "교육",
                },
                new DutyCategoryOption
                {
                    Code = 69,
                    GroupCode = 10,
                    Name = "교수",
                },
                new DutyCategoryOption
                {
                    Code = 70,
                    GroupCode = 10,
                    Name = "강사",
                },
                new DutyCategoryOption
                {
                    Code = 71,
                    GroupCode = 10,
                    Name = "저술",
                },
                new DutyCategoryOption
                {
                    Code = 72,
                    GroupCode = 10,
                    Name = "번역",
                },
            ]
        },
    ];

    private static readonly IReadOnlyDictionary<int, DutyCategoryOption> OptionByCode = GroupItems
        .SelectMany(group => group.Options)
        .ToDictionary(option => option.Code);

    public static IReadOnlyList<DutyCategoryGroup> Groups => GroupItems;

    public static IReadOnlyList<int> DefaultSelectedCodes { get; } = [ClientDutyCode];

    public static IReadOnlyList<int> NormalizeSelectedCodes(IEnumerable<int>? selectedCodes)
    {
        var normalized = (selectedCodes ?? [])
            .Where(OptionByCode.ContainsKey)
            .Distinct()
            .OrderBy(code => code)
            .ToArray();

        return normalized.Length == 0 ? [ClientDutyCode] : normalized;
    }

    public static IReadOnlySet<string> ResolveDutyNames(IEnumerable<int>? selectedCodes)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var code in NormalizeSelectedCodes(selectedCodes))
        {
            if (OptionByCode.TryGetValue(code, out var option))
            {
                set.Add(option.Name);
            }
        }

        return set;
    }

    public static string BuildDutyUrl(IEnumerable<int>? selectedCodes)
    {
        var dutyParameter = string.Join(",", NormalizeSelectedCodes(selectedCodes));
        return $"https://www.gamejob.co.kr/Recruit/joblist?menucode=duty&duty={dutyParameter}";
    }
}

