namespace samvaad_backend.Models.DTOs.Messages;

public record MessageDto(
    Guid Id,
    Guid SenderId,
    string SenderDisplayName,
    string? SenderAvatarUrl,
    string Content,
    DateTime CreatedAt,
    bool IsMine);
