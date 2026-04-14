using GameJobNotifier.App.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameJobNotifier.App.Services;

public sealed class MonitoringBackgroundService(
    IJobSyncService jobSyncService,
    ISettingsService settingsService,
    ICheckRequestQueue checkRequestQueue,
    ILogger<MonitoringBackgroundService> logger) : BackgroundService
{
    private readonly IJobSyncService _jobSyncService = jobSyncService;
    private readonly ISettingsService _settingsService = settingsService;
    private readonly ICheckRequestQueue _checkRequestQueue = checkRequestQueue;
    private readonly ILogger<MonitoringBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunCheckAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var settings = (await _settingsService.GetAsync(stoppingToken)).Sanitize();
            var interval = TimeSpan.FromMinutes(settings.CheckIntervalMinutes);

            using var waitTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            waitTokenSource.CancelAfter(interval);

            try
            {
                await _checkRequestQueue.WaitForCheckAsync(waitTokenSource.Token);

                while (_checkRequestQueue.TryDequeue())
                {
                    // Drain duplicate manual requests before running the next sync.
                }
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                // Interval elapsed. Proceed with scheduled check.
            }

            await RunCheckAsync(stoppingToken);
        }
    }

    private async Task RunCheckAsync(CancellationToken cancellationToken)
    {
        var result = await _jobSyncService.SyncAsync(cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Monitoring cycle failed: {Error}", result.ErrorMessage);
        }
    }
}
