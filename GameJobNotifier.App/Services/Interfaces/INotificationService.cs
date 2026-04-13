using GameJobNotifier.App.Models;

namespace GameJobNotifier.App.Services.Interfaces;

public interface INotificationService
{
    Task NotifyNewPostingAsync(
        JobPosting posting,
        AppSettings settings,
        CancellationToken cancellationToken = default);
}
