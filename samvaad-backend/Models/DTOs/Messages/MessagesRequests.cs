namespace samvaad_backend.Models.DTOs.Messages;

public record PagedMessagesDto(List<MessageDto> Messages, bool HasMore);

public record SendMessageRequest(string Content);

public record StartDirectConversationRequest(string TargetUsername);
