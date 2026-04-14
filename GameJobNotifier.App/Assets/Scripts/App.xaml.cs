using GameJobNotifier.App.Infrastructure;
using GameJobNotifier.App.Services;
using GameJobNotifier.App.Services.Interfaces;
using GameJobNotifier.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameJobNotifier.App;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
        WindowsAppIdentity.EnsureExplicitAppUserModelId();

        AppPaths.EnsureCreated();
        _host = CreateHostBuilder().Build();
        await _host.StartAsync();

        var tray = _host.Services.GetRequiredService<ITrayIconService>();
        tray.OpenRequested += HandleTrayOpenRequested;
        tray.CheckRequested += HandleTrayCheckRequested;
        tray.ExitRequested += HandleTrayExitRequested;
        tray.Initialize();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;

        var settings = (await _host.Services.GetRequiredService<ISettingsService>().GetAsync()).Sanitize();
        var startupService = _host.Services.GetRequiredService<IWindowsStartupService>();
        if (!startupService.TryConfigure(settings.StartInBackground, out var startupError))
        {
            _host.Services.GetRequiredService<ILogger<App>>()
                .LogWarning("Windows startup sync failed: {Error}", startupError);
        }

        if (settings.StartInBackground)
        {
            tray.ShowBalloon("GameJobNotifier", "트레이에서 백그라운드 실행 중");
            return;
        }

        mainWindow.Show();
    }

    protected override async void OnExit(System.Windows.ExitEventArgs e)
    {
        if (_host is not null)
        {
            try
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }
            catch
            {
                // Ignore shutdown exceptions so the app can terminate cleanly.
            }

            _host.Dispose();
            _host = null;
        }

        base.OnExit(e);
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddDebug();
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton(TimeProvider.System);

                services.AddSingleton<ISettingsService, JsonSettingsService>();
                services.AddSingleton<IRuntimeStateService, JsonRuntimeStateService>();
                services.AddSingleton<IGameJobHtmlParser, GameJobHtmlParser>();
                services.AddSingleton<IJobPostingRepository, SqliteJobPostingRepository>();
                services.AddSingleton<IFilterCriteriaFactory, FilterCriteriaFactory>();
                services.AddSingleton<ITrayIconService, TrayIconService>();
                services.AddSingleton<INotificationService, NotificationService>();
                services.AddSingleton<IWindowsStartupService, WindowsStartupService>();
                services.AddSingleton<ISyncEventHub, SyncEventHub>();
                services.AddSingleton<ICheckRequestQueue, CheckRequestQueue>();
                services.AddSingleton<IJobSyncService, JobSyncService>();

                services.AddHttpClient<IJobCollector, GameJobHttpCollector>(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                        "(KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
                    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ko-KR,ko;q=0.9,en-US;q=0.8");
                });

                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();

                services.AddHostedService<MonitoringBackgroundService>();
            });
    }

    private void HandleTrayOpenRequested()
    {
        Dispatcher.Invoke(() =>
        {
            if (MainWindow is null)
            {
                return;
            }

            if (!MainWindow.IsVisible)
            {
                MainWindow.Show();
            }

            if (MainWindow.WindowState == System.Windows.WindowState.Minimized)
            {
                MainWindow.WindowState = System.Windows.WindowState.Normal;
            }

            MainWindow.Activate();
            MainWindow.Topmost = true;
            MainWindow.Topmost = false;
            MainWindow.Focus();
        });
    }

    private void HandleTrayCheckRequested()
    {
        _host?.Services.GetRequiredService<ICheckRequestQueue>().RequestCheck();
    }

    private void HandleTrayExitRequested()
    {
        Dispatcher.Invoke(() =>
        {
            if (MainWindow is MainWindow window)
            {
                window.ExitRequested = true;
                window.Close();
            }

            Shutdown();
        });
    }
}
