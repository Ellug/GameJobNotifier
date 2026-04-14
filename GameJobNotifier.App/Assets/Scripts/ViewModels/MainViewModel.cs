using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameJobNotifier.App.Infrastructure;
using GameJobNotifier.App.Models;
using GameJobNotifier.App.Services.Interfaces;
using GameJobNotifier.App.ViewModels.Containers;
using Microsoft.Extensions.Logging;

namespace GameJobNotifier.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IRuntimeStateService _runtimeStateService;
    private readonly ICheckRequestQueue _checkRequestQueue;
    private readonly ISyncEventHub _syncEventHub;
    private readonly INotificationService _notificationService;
    private readonly IWindowsStartupService _windowsStartupService;
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty]
    private int _checkIntervalMinutes = 10;

    [ObservableProperty]
    private bool _enableToastNotification = true;

    [ObservableProperty]
    private bool _enableTrayBalloon = true;

    [ObservableProperty]
    private bool _startInBackground;

    [ObservableProperty]
    private string _statusText = "대기 중";

    [ObservableProperty]
    private string _lastRunText = "최근 검사: 없음";

    [ObservableProperty]
    private bool _isSaving;

    public ObservableCollection<ActivityLogEntry> ActivityLogs { get; } = [];

    public DutyFilterContainerViewModel DutyFilter { get; } = new();

    public MultiSelectFilterContainerViewModel RegionFilter { get; } =
        new("선택 지역", "전체", FilterOptionCatalog.RegionOptions);

    public MultiSelectFilterContainerViewModel GameFieldFilter { get; } =
        new("선택 게임분야", "전체", FilterOptionCatalog.GameFieldOptions);

    public MultiSelectFilterContainerViewModel WorkConditionFilter { get; } =
        new("선택 근무조건", "전체", FilterOptionCatalog.WorkConditionOptions);

    public MultiSelectFilterContainerViewModel QualificationFilter { get; } =
        new("선택 지원자격", "전체", FilterOptionCatalog.QualificationOptions);

    public MainViewModel(
        ISettingsService settingsService,
        IRuntimeStateService runtimeStateService,
        ICheckRequestQueue checkRequestQueue,
        ISyncEventHub syncEventHub,
        INotificationService notificationService,
        IWindowsStartupService windowsStartupService,
        ILogger<MainViewModel> logger)
    {
        _settingsService = settingsService;
        _runtimeStateService = runtimeStateService;
        _checkRequestQueue = checkRequestQueue;
        _syncEventHub = syncEventHub;
        _notificationService = notificationService;
        _windowsStartupService = windowsStartupService;
        _logger = logger;

        _syncEventHub.SyncCompleted += HandleSyncCompleted;
        _ = LoadSettingsAsync();
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        var snapshot = BuildSelectionSnapshot();
        if (snapshot.DutyCodes.Count == 0)
        {
            StatusText = "저장 실패: 직종을 1개 이상 선택하세요.";
            AppendLog("설정 저장 실패 - 직종 미선택");
            return;
        }

        try
        {
            IsSaving = true;

            var settings = new AppSettings
            {
                CheckIntervalMinutes = CheckIntervalMinutes,
                EnableToastNotification = EnableToastNotification,
                EnableTrayBalloon = EnableTrayBalloon,
                StartInBackground = StartInBackground,
                SelectedDutyCodes = snapshot.DutyCodes,
                SelectedRegions = snapshot.Regions,
                SelectedGameFields = snapshot.GameFields,
                SelectedWorkConditions = snapshot.WorkConditions,
                SelectedQualifications = snapshot.Qualifications
            }.Sanitize();

            await _settingsService.SaveAsync(settings);
            ApplySettings(settings);

            if (!_windowsStartupService.TryConfigure(settings.StartInBackground, out var startupError))
            {
                AppendLog($"Windows 시작프로그램 설정 실패 - {startupError}");
            }
            else
            {
                AppendLog(settings.StartInBackground
                    ? "Windows 시작프로그램 등록됨 (부팅 시 백그라운드 시작)"
                    : "Windows 시작프로그램 해제됨");
            }

            StatusText = "설정 저장 완료";
            AppendLog(
                $"설정을 저장했습니다. (직종 {settings.SelectedDutyCodes.Count}, 지역 {settings.SelectedRegions.Count}, 게임분야 {settings.SelectedGameFields.Count}, 근무조건 {settings.SelectedWorkConditions.Count}, 지원자격 {settings.SelectedQualifications.Count})");

            _checkRequestQueue.RequestCheck();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings.");
            StatusText = $"저장 실패: {ex.Message}";
            AppendLog($"설정 저장 실패 - {ex.Message}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void RunNow()
    {
        _checkRequestQueue.RequestCheck();
        AppendLog("수동 검사 요청됨");
        StatusText = "수동 검사 요청됨";
    }

    [RelayCommand]
    private void OpenDataFolder()
    {
        AppPaths.EnsureCreated();

        Process.Start(new ProcessStartInfo
        {
            FileName = AppPaths.BaseDirectory,
            UseShellExecute = true
        });
    }

    [RelayCommand]
    private void OpenNotificationSettings()
    {
        var opened = TryOpenUri("ms-settings:notifications") ||
                     TryOpenUri("ms-settings:notifications-app");

        if (opened)
        {
            StatusText = "Windows 알림 설정 열기";
            AppendLog("Windows 알림 설정 화면을 열었습니다.");
            return;
        }

        StatusText = "알림 설정 열기 실패";
        AppendLog("Windows 알림 설정 화면 열기 실패");
        System.Windows.MessageBox.Show(
            "Windows 알림 설정 화면을 열지 못했습니다.\n설정 > 시스템 > 알림에서 GameJobNotifier 알림 허용을 확인해 주세요.",
            "알림 설정",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Warning);
    }

    [RelayCommand]
    private async Task TestNotificationAsync()
    {
        if (!EnableToastNotification && !EnableTrayBalloon)
        {
            StatusText = "알림 테스트 실패: Toast 또는 트레이 풍선 알림을 켜주세요.";
            AppendLog("알림 테스트 실패 - 알림 옵션이 모두 꺼져 있음");
            System.Windows.MessageBox.Show(
                "알림 옵션이 모두 꺼져 있습니다.\n'Toast 알림' 또는 '트레이 풍선 알림'을 켜고 다시 테스트해 주세요.",
                "알림 테스트",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
            return;
        }

        var testPosting = new JobPosting
        {
            JobId = $"TEST-{DateTime.Now:yyyyMMddHHmmss}",
            DetailUrl = "https://www.gamejob.co.kr/",
            Title = "[테스트] 알림 동작 확인",
            Company = "GameJobNotifier",
            CareerText = "신입/경력무관",
            LocationText = "서울"
        };

        var settings = new AppSettings
        {
            CheckIntervalMinutes = CheckIntervalMinutes,
            EnableToastNotification = EnableToastNotification,
            EnableTrayBalloon = EnableTrayBalloon,
            StartInBackground = StartInBackground,
            SelectedDutyCodes = DutyFilter.GetSelectedCodes(),
            SelectedRegions = RegionFilter.GetSelectedKeys(),
            SelectedGameFields = GameFieldFilter.GetSelectedKeys(),
            SelectedWorkConditions = WorkConditionFilter.GetSelectedKeys(),
            SelectedQualifications = QualificationFilter.GetSelectedKeys()
        }.Sanitize();

        try
        {
            await _notificationService.NotifyPostingAsync(testPosting, settings, "신규 공고");
            StatusText = "알림 테스트 전송 완료";
            AppendLog($"알림 테스트 전송 완료 (Toast={settings.EnableToastNotification}, Tray={settings.EnableTrayBalloon})");
            System.Windows.MessageBox.Show(
                $"알림 테스트를 전송했습니다.\nToast={settings.EnableToastNotification}, Tray={settings.EnableTrayBalloon}\n\n" +
                "화면에 표시되지 않으면 Windows 알림 설정(집중 지원/앱 알림 허용)을 확인해 주세요.",
                "알림 테스트",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test notification.");
            StatusText = $"알림 테스트 실패: {ex.Message}";
            AppendLog($"알림 테스트 실패 - {ex.Message}");
            System.Windows.MessageBox.Show(
                $"알림 테스트 중 오류가 발생했습니다.\n{ex.Message}",
                "알림 테스트 실패",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void OpenLogItem(ActivityLogEntry? entry)
    {
        if (entry is null || !entry.HasUrl)
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = entry.Url!,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open log item URL.");
            AppendLog($"로그 링크 열기 실패 - {ex.Message}");
        }
    }

    private FilterSelectionSnapshot BuildSelectionSnapshot()
    {
        return new FilterSelectionSnapshot
        {
            DutyCodes = DutyFilter.GetSelectedCodes(),
            Regions = RegionFilter.GetSelectedKeys(),
            GameFields = GameFieldFilter.GetSelectedKeys(),
            WorkConditions = WorkConditionFilter.GetSelectedKeys(),
            Qualifications = QualificationFilter.GetSelectedKeys()
        };
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            var settings = await _settingsService.GetAsync();
            var runtimeState = await _runtimeStateService.GetAsync();
            ApplySettings(settings);

            if (runtimeState.LastSuccessfulCheckUtc is not null)
            {
                LastRunText = $"최근 검사: {runtimeState.LastSuccessfulCheckUtc.Value.LocalDateTime:yyyy-MM-dd HH:mm:ss}";
            }

            AppendLog("설정을 불러왔습니다.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings.");
            AppendLog($"설정 로드 실패 - {ex.Message}");
        }
    }

    private void ApplySettings(AppSettings settings)
    {
        void Apply()
        {
            CheckIntervalMinutes = settings.CheckIntervalMinutes;
            EnableToastNotification = settings.EnableToastNotification;
            EnableTrayBalloon = settings.EnableTrayBalloon;
            StartInBackground = settings.StartInBackground;

            DutyFilter.ApplySelection(settings.SelectedDutyCodes);
            RegionFilter.ApplySelection(settings.SelectedRegions);
            GameFieldFilter.ApplySelection(settings.SelectedGameFields);
            WorkConditionFilter.ApplySelection(settings.SelectedWorkConditions);
            QualificationFilter.ApplySelection(settings.SelectedQualifications);
        }

        if (System.Windows.Application.Current.Dispatcher.CheckAccess())
        {
            Apply();
            return;
        }

        System.Windows.Application.Current.Dispatcher.Invoke(Apply);
    }

    private void HandleSyncCompleted(object? sender, SyncResult result)
    {
        void Apply()
        {
            LastRunText = $"최근 검사: {result.CompletedAtUtc.LocalDateTime:yyyy-MM-dd HH:mm:ss}";

            if (!result.IsSuccess)
            {
                StatusText = $"오류: {result.ErrorMessage}";
                AppendLog($"검사 실패 - {result.ErrorMessage}");
                return;
            }

            StatusText =
                $"완료 | 수집 {result.FetchedCount}건, 조건일치 {result.MatchedCount}건, 신규 {result.NewCount}건";
            AppendLog(
                $"검사 완료 ({result.CollectorName}) - 신규 {result.NewCount}, 삭제 {result.HiddenCount}, 복원 {result.RestoredCount}");

            AppendChangeDetails(result.Changes);
        }

        if (System.Windows.Application.Current.Dispatcher.CheckAccess())
        {
            Apply();
            return;
        }

        System.Windows.Application.Current.Dispatcher.Invoke(Apply);
    }

    private void AppendChangeDetails(IReadOnlyList<JobChange> changes)
    {
        if (changes.Count == 0)
        {
            AppendLog("변경 공고 없음");
            return;
        }

        const int maxDetailLogs = 40;
        foreach (var change in changes.Take(maxDetailLogs))
        {
            var label = change.ChangeType switch
            {
                JobChangeType.Added => "신규",
                JobChangeType.Hidden => "비노출",
                JobChangeType.Restored => "복원",
                _ => "변경"
            };

            var posting = change.Posting;
            AppendLog($"{label} | [{posting.JobId}] {posting.Company} | {posting.Title}", posting.DetailUrl);
        }

        if (changes.Count > maxDetailLogs)
        {
            AppendLog($"변경 상세 로그 생략 {changes.Count - maxDetailLogs}건");
        }
    }

    private void AppendLog(string message, string? url = null)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        ActivityLogs.Insert(0, new ActivityLogEntry
        {
            Text = line,
            Url = url
        });

        const int maxLogCount = 250;
        while (ActivityLogs.Count > maxLogCount)
        {
            ActivityLogs.RemoveAt(ActivityLogs.Count - 1);
        }
    }

    private bool TryOpenUri(string uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = uri,
                UseShellExecute = true
            });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to open URI: {Uri}", uri);
            return false;
        }
    }
}
