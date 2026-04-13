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
                    PostingCount = 267
                },
                new DutyCategoryOption
                {
                    Code = 2,
                    GroupCode = 1,
                    Name = "게임개발(모바일)",
                    PostingCount = 356
                },
                new DutyCategoryOption
                {
                    Code = 3,
                    GroupCode = 1,
                    Name = "게임AI 개발",
                    PostingCount = 97
                },
                new DutyCategoryOption
                {
                    Code = 4,
                    GroupCode = 1,
                    Name = "인터페이스 디자인",
                    PostingCount = 184
                },
                new DutyCategoryOption
                {
                    Code = 5,
                    GroupCode = 1,
                    Name = "원화",
                    PostingCount = 276
                },
                new DutyCategoryOption
                {
                    Code = 6,
                    GroupCode = 1,
                    Name = "모델링",
                    PostingCount = 257
                },
                new DutyCategoryOption
                {
                    Code = 7,
                    GroupCode = 1,
                    Name = "애니메이션",
                    PostingCount = 271
                },
                new DutyCategoryOption
                {
                    Code = 8,
                    GroupCode = 1,
                    Name = "이펙트·FX",
                    PostingCount = 202
                },
                new DutyCategoryOption
                {
                    Code = 9,
                    GroupCode = 1,
                    Name = "게임기획",
                    PostingCount = 505
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
                    PostingCount = 24
                },
                new DutyCategoryOption
                {
                    Code = 11,
                    GroupCode = 2,
                    Name = "영상제작·편집",
                    PostingCount = 57
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
                    PostingCount = 58
                },
                new DutyCategoryOption
                {
                    Code = 13,
                    GroupCode = 3,
                    Name = "플랫폼 디자인",
                    PostingCount = 29
                },
                new DutyCategoryOption
                {
                    Code = 14,
                    GroupCode = 3,
                    Name = "BX·브랜드 디자인",
                    PostingCount = 6
                },
                new DutyCategoryOption
                {
                    Code = 15,
                    GroupCode = 3,
                    Name = "플랫폼 기획",
                    PostingCount = 11
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
                    PostingCount = 116
                },
                new DutyCategoryOption
                {
                    Code = 17,
                    GroupCode = 4,
                    Name = "네트워크",
                    PostingCount = 39
                },
                new DutyCategoryOption
                {
                    Code = 18,
                    GroupCode = 4,
                    Name = "엔진",
                    PostingCount = 77
                },
                new DutyCategoryOption
                {
                    Code = 19,
                    GroupCode = 4,
                    Name = "시스템·DB",
                    PostingCount = 57
                },
                new DutyCategoryOption
                {
                    Code = 20,
                    GroupCode = 4,
                    Name = "보안",
                    PostingCount = 19
                },
                new DutyCategoryOption
                {
                    Code = 21,
                    GroupCode = 4,
                    Name = "클라우드",
                    PostingCount = 23
                },
                new DutyCategoryOption
                {
                    Code = 22,
                    GroupCode = 4,
                    Name = "결제·PG",
                    PostingCount = 7
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
                    PostingCount = 107
                },
                new DutyCategoryOption
                {
                    Code = 24,
                    GroupCode = 5,
                    Name = "QA·테스터",
                    PostingCount = 76
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
                    PostingCount = 75
                },
                new DutyCategoryOption
                {
                    Code = 26,
                    GroupCode = 6,
                    Name = "사업기획(해외)",
                    PostingCount = 71
                },
                new DutyCategoryOption
                {
                    Code = 27,
                    GroupCode = 6,
                    Name = "전략기획",
                    PostingCount = 41
                },
                new DutyCategoryOption
                {
                    Code = 28,
                    GroupCode = 6,
                    Name = "사업관리",
                    PostingCount = 87
                },
                new DutyCategoryOption
                {
                    Code = 29,
                    GroupCode = 6,
                    Name = "경영분석",
                    PostingCount = 5
                },
                new DutyCategoryOption
                {
                    Code = 30,
                    GroupCode = 6,
                    Name = "정보·빅데이터 분석",
                    PostingCount = 68
                },
                new DutyCategoryOption
                {
                    Code = 31,
                    GroupCode = 6,
                    Name = "리서치",
                    PostingCount = 19
                },
                new DutyCategoryOption
                {
                    Code = 32,
                    GroupCode = 6,
                    Name = "임원·CEO",
                    PostingCount = 0
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
                    PostingCount = 107
                },
                new DutyCategoryOption
                {
                    Code = 34,
                    GroupCode = 7,
                    Name = "홍보",
                    PostingCount = 64
                },
                new DutyCategoryOption
                {
                    Code = 35,
                    GroupCode = 7,
                    Name = "해외 마케팅",
                    PostingCount = 54
                },
                new DutyCategoryOption
                {
                    Code = 36,
                    GroupCode = 7,
                    Name = "프로모션",
                    PostingCount = 41
                },
                new DutyCategoryOption
                {
                    Code = 37,
                    GroupCode = 7,
                    Name = "광고기획",
                    PostingCount = 44
                },
                new DutyCategoryOption
                {
                    Code = 38,
                    GroupCode = 7,
                    Name = "취재",
                    PostingCount = 0
                },
                new DutyCategoryOption
                {
                    Code = 39,
                    GroupCode = 7,
                    Name = "게임기자",
                    PostingCount = 0
                },
                new DutyCategoryOption
                {
                    Code = 40,
                    GroupCode = 7,
                    Name = "투자배급",
                    PostingCount = 0
                },
                new DutyCategoryOption
                {
                    Code = 41,
                    GroupCode = 7,
                    Name = "CRM",
                    PostingCount = 7
                },
                new DutyCategoryOption
                {
                    Code = 42,
                    GroupCode = 7,
                    Name = "제휴",
                    PostingCount = 5
                },
                new DutyCategoryOption
                {
                    Code = 43,
                    GroupCode = 7,
                    Name = "프로게이머",
                    PostingCount = 0
                },
                new DutyCategoryOption
                {
                    Code = 44,
                    GroupCode = 7,
                    Name = "E-Sports",
                    PostingCount = 5
                },
                new DutyCategoryOption
                {
                    Code = 45,
                    GroupCode = 7,
                    Name = "방송",
                    PostingCount = 6
                },
                new DutyCategoryOption
                {
                    Code = 46,
                    GroupCode = 7,
                    Name = "퍼블리싱",
                    PostingCount = 39
                },
                new DutyCategoryOption
                {
                    Code = 47,
                    GroupCode = 7,
                    Name = "소싱/라이선싱",
                    PostingCount = 12
                },
                new DutyCategoryOption
                {
                    Code = 48,
                    GroupCode = 7,
                    Name = "채널링",
                    PostingCount = 5
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
                    PostingCount = 6
                },
                new DutyCategoryOption
                {
                    Code = 50,
                    GroupCode = 8,
                    Name = "해외영업",
                    PostingCount = 14
                },
                new DutyCategoryOption
                {
                    Code = 51,
                    GroupCode = 8,
                    Name = "영업 관리",
                    PostingCount = 4
                },
                new DutyCategoryOption
                {
                    Code = 52,
                    GroupCode = 8,
                    Name = "해외지사 관리",
                    PostingCount = 3
                },
                new DutyCategoryOption
                {
                    Code = 53,
                    GroupCode = 8,
                    Name = "기술영업",
                    PostingCount = 3
                },
                new DutyCategoryOption
                {
                    Code = 54,
                    GroupCode = 8,
                    Name = "PC방 영업·관리",
                    PostingCount = 0
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
                    PostingCount = 24
                },
                new DutyCategoryOption
                {
                    Code = 56,
                    GroupCode = 9,
                    Name = "재무",
                    PostingCount = 14
                },
                new DutyCategoryOption
                {
                    Code = 57,
                    GroupCode = 9,
                    Name = "회계",
                    PostingCount = 11
                },
                new DutyCategoryOption
                {
                    Code = 58,
                    GroupCode = 9,
                    Name = "경리",
                    PostingCount = 6
                },
                new DutyCategoryOption
                {
                    Code = 59,
                    GroupCode = 9,
                    Name = "세무",
                    PostingCount = 4
                },
                new DutyCategoryOption
                {
                    Code = 60,
                    GroupCode = 9,
                    Name = "통계",
                    PostingCount = 3
                },
                new DutyCategoryOption
                {
                    Code = 61,
                    GroupCode = 9,
                    Name = "주식IR",
                    PostingCount = 4
                },
                new DutyCategoryOption
                {
                    Code = 62,
                    GroupCode = 9,
                    Name = "CFO",
                    PostingCount = 0
                },
                new DutyCategoryOption
                {
                    Code = 63,
                    GroupCode = 9,
                    Name = "CFA",
                    PostingCount = 0
                },
                new DutyCategoryOption
                {
                    Code = 64,
                    GroupCode = 9,
                    Name = "인사",
                    PostingCount = 24
                },
                new DutyCategoryOption
                {
                    Code = 65,
                    GroupCode = 9,
                    Name = "법무",
                    PostingCount = 2
                },
                new DutyCategoryOption
                {
                    Code = 66,
                    GroupCode = 9,
                    Name = "총무",
                    PostingCount = 26
                },
                new DutyCategoryOption
                {
                    Code = 67,
                    GroupCode = 9,
                    Name = "구매",
                    PostingCount = 11
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
                    PostingCount = 23
                },
                new DutyCategoryOption
                {
                    Code = 69,
                    GroupCode = 10,
                    Name = "교수",
                    PostingCount = 6
                },
                new DutyCategoryOption
                {
                    Code = 70,
                    GroupCode = 10,
                    Name = "강사",
                    PostingCount = 28
                },
                new DutyCategoryOption
                {
                    Code = 71,
                    GroupCode = 10,
                    Name = "저술",
                    PostingCount = 0
                },
                new DutyCategoryOption
                {
                    Code = 72,
                    GroupCode = 10,
                    Name = "번역",
                    PostingCount = 26
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
