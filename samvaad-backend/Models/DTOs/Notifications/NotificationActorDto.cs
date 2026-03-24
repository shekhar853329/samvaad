namespace samvaad_backend.Models.DTOs.Notifications;

public record NotificationActorDto(
    Guid Id,
    string Username,
    string DisplayName,
    string? AvatarUrl,
    bool IsVerified);
