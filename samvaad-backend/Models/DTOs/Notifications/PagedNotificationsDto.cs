namespace samvaad_backend.Models.DTOs.Notifications;

public record PagedNotificationsDto(
    List<NotificationDto> Notifications,
    bool HasMore,
    int UnreadCount);
