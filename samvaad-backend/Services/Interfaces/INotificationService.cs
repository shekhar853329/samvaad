using samvaad_backend.Models.DTOs.Notifications;
using samvaad_backend.Models.Enums;

namespace samvaad_backend.Services.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// Fetch paginated notifications for a user.
    /// typeFilter is the string representation of NotificationType (e.g. "Like"), or null for all.
    /// </summary>
    Task<PagedNotificationsDto> GetNotificationsAsync(Guid userId, string? typeFilter, int page = 1, int pageSize = 20);

    Task<int> GetUnreadCountAsync(Guid userId);

    Task MarkAsReadAsync(Guid notificationId, Guid userId);

    Task MarkAllAsReadAsync(Guid userId);

    /// <summary>Internal helper called by other services to insert a notification. No-ops when recipient == actor.</summary>
    Task CreateAsync(Guid recipientId, Guid? actorId, NotificationType type, Guid? postId = null, string? systemMessage = null);
}
