using samvaad_backend.Models.DTOs.Messages;

namespace samvaad_backend.Services.Interfaces;

public interface IMessageService
{
    Task<List<ConversationDto>> GetConversationsAsync(Guid userId);
    Task<ConversationDto> GetOrCreateDirectAsync(Guid userId, string targetUsername);
    Task<PagedMessagesDto> GetMessagesAsync(Guid conversationId, Guid userId, int page = 1, int pageSize = 30);
    Task<MessageDto> SendMessageAsync(Guid conversationId, Guid senderId, string content);
    Task MarkReadAsync(Guid conversationId, Guid userId);
}
