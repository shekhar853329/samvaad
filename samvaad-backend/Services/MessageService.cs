using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using samvaad_backend.Common;
using samvaad_backend.Data;
using samvaad_backend.Hubs;
using samvaad_backend.Models.DTOs.Messages;
using samvaad_backend.Models.Entities;
using samvaad_backend.Services.Interfaces;

namespace samvaad_backend.Services;

public class MessageService(AppDbContext db, IHubContext<ChatHub> hubContext) : IMessageService
{
    public async Task<List<ConversationDto>> GetConversationsAsync(Guid userId)
    {
        var rows = await db.ConversationParticipants
            .Where(cp => cp.UserId == userId)
            .Select(cp => new
            {
                cp.LastReadAt,
                Conversation = cp.Conversation,
                OtherParticipants = cp.Conversation.Participants
                    .Where(p => p.UserId != userId)
                    .Select(p => new ConversationParticipantDto(
                        p.UserId, p.User.Username, p.User.DisplayName, p.User.AvatarUrl)),
                LastMessage = cp.Conversation.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new { m.Content, m.Sender.DisplayName, m.CreatedAt, m.SenderId })
                    .FirstOrDefault(),
                UnreadCount = cp.Conversation.Messages
                    .Count(m => m.CreatedAt > (cp.LastReadAt ?? DateTime.MinValue) && m.SenderId != userId)
            })
            .OrderByDescending(x => x.Conversation.LastMessageAt)
            .ToListAsync();

        return rows.Select(r => new ConversationDto(
            r.Conversation.Id,
            r.Conversation.IsGroup,
            r.Conversation.Name,
            r.OtherParticipants.ToList(),
            r.LastMessage?.Content,
            r.LastMessage?.DisplayName,
            r.LastMessage?.CreatedAt,
            r.UnreadCount
        )).ToList();
    }

    public async Task<ConversationDto> GetOrCreateDirectAsync(Guid userId, string targetUsername)
    {
        var target = await db.Users.FirstOrDefaultAsync(u => u.Username == targetUsername)
            ?? throw new AppException("User not found", 404);

        if (target.Id == userId)
            throw new AppException("Cannot message yourself");

        // Find existing non-group conversation shared by both users
        var existingId = await (
            from cp1 in db.ConversationParticipants
            where cp1.UserId == userId
            join cp2 in db.ConversationParticipants on cp1.ConversationId equals cp2.ConversationId
            where cp2.UserId == target.Id
            join c in db.Conversations on cp1.ConversationId equals c.Id
            where !c.IsGroup
            select c.Id
        ).FirstOrDefaultAsync();

        Guid conversationId;
        if (existingId != default)
        {
            conversationId = existingId;
        }
        else
        {
            var conv = new Conversation
            {
                Id = Guid.NewGuid(),
                IsGroup = false,
                CreatedByUserId = userId,
                LastMessageAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            db.Conversations.Add(conv);
            db.ConversationParticipants.Add(new ConversationParticipant
            {
                ConversationId = conv.Id, UserId = userId, JoinedAt = DateTime.UtcNow
            });
            db.ConversationParticipants.Add(new ConversationParticipant
            {
                ConversationId = conv.Id, UserId = target.Id, JoinedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
            conversationId = conv.Id;
        }

        var convs = await GetConversationsAsync(userId);
        return convs.First(c => c.Id == conversationId);
    }

    public async Task<PagedMessagesDto> GetMessagesAsync(Guid conversationId, Guid userId, int page = 1, int pageSize = 30)
    {
        var isParticipant = await db.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);
        if (!isParticipant)
            throw new AppException("Not a participant", 403);

        var fetch = pageSize + 1;
        var items = await db.Messages
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(fetch)
            .Select(m => new MessageDto(
                m.Id,
                m.SenderId,
                m.Sender.DisplayName,
                m.Sender.AvatarUrl,
                m.Content,
                m.CreatedAt,
                m.SenderId == userId))
            .ToListAsync();

        var hasMore = items.Count == fetch;
        // Return in ascending order (oldest first) for display
        var result = items.Take(pageSize).OrderBy(m => m.CreatedAt).ToList();
        return new PagedMessagesDto(result, hasMore);
    }

    public async Task<MessageDto> SendMessageAsync(Guid conversationId, Guid senderId, string content)
    {
        var isParticipant = await db.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == senderId);
        if (!isParticipant)
            throw new AppException("Not a participant", 403);

        if (string.IsNullOrWhiteSpace(content))
            throw new AppException("Message content cannot be empty");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        db.Messages.Add(message);

        await db.Conversations
            .Where(c => c.Id == conversationId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.LastMessageAt, DateTime.UtcNow));

        await db.SaveChangesAsync();

        var sender = await db.Users.FindAsync(senderId);
        var dto = new MessageDto(message.Id, senderId, sender!.DisplayName, sender.AvatarUrl,
                     message.Content, message.CreatedAt, true);

        // Broadcast to all participants in the conversation room
        // Each recipient receives isMine=false
        var recipientDto = dto with { IsMine = false };
        await hubContext.Clients.Group(conversationId.ToString())
            .SendAsync("NewMessage", new
            {
                conversationId = conversationId.ToString(),
                message = recipientDto
            });

        return dto;
    }

    public async Task MarkReadAsync(Guid conversationId, Guid userId)
    {
        await db.ConversationParticipants
            .Where(cp => cp.ConversationId == conversationId && cp.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(cp => cp.LastReadAt, DateTime.UtcNow));
    }
}
