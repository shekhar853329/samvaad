namespace samvaad_backend.Models.DTOs.Messages;

public record ConversationParticipantDto(
    Guid UserId,
    string Username,
    string DisplayName,
    string? AvatarUrl);
