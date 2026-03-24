namespace samvaad_backend.Models.DTOs.Messages;

public record ConversationDto(
    Guid Id,
    bool IsGroup,
    string? Name,
    List<ConversationParticipantDto> OtherParticipants,
    string? LastMessageContent,
    string? LastMessageSenderName,
    DateTime? LastMessageAt,
    int UnreadCount);
