using GameJobNotifier.App.Models;

namespace GameJobNotifier.App.Services.Interfaces;

public interface IJobCollector
{
    Task<JobCollectionResult> CollectAsync(Uri targetUrl, CancellationToken cancellationToken = default);
}
