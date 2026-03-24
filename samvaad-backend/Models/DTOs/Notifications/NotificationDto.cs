namespace samvaad_backend.Models.DTOs.Notifications;

public record NotificationDto(
    Guid Id,
    string Type,
    bool IsRead,
    DateTime CreatedAt,
    NotificationActorDto? Actor,
    Guid? PostId,
    string? PostSnippet,
    string? SystemMessage);
