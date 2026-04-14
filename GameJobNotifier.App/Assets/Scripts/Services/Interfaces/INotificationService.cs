using GameJobNotifier.App.Models;

namespace GameJobNotifier.App.Services.Interfaces;

public interface INotificationService
{
    Task NotifyPostingAsync(
        JobPosting posting,
        AppSettings settings,
        string notificationTitle,
        CancellationToken cancellationToken = default);
}
