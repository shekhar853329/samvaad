using Microsoft.EntityFrameworkCore;
using samvaad_backend.Common;
using samvaad_backend.Data;
using samvaad_backend.Models.DTOs.Notifications;
using samvaad_backend.Models.Entities;
using samvaad_backend.Models.Enums;
using samvaad_backend.Services.Interfaces;

namespace samvaad_backend.Services;

public class NotificationService(AppDbContext db) : INotificationService
{
    public async Task CreateAsync(Guid recipientId, Guid? actorId, NotificationType type, Guid? postId = null, string? systemMessage = null)
    {
        // Don't notify users about their own actions
        if (actorId.HasValue && actorId.Value == recipientId) return;

        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            RecipientId = recipientId,
            ActorId = actorId,
            Type = type,
            PostId = postId,
            IsRead = false,
            SystemMessage = systemMessage,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }

    public async Task<PagedNotificationsDto> GetNotificationsAsync(Guid userId, string? typeFilter, int page = 1, int pageSize = 20)
    {
        var query = db.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientId == userId);

        if (!string.IsNullOrWhiteSpace(typeFilter) &&
            Enum.TryParse<NotificationType>(typeFilter, ignoreCase: true, out var parsedType))
        {
            query = query.Where(n => n.Type == parsedType);
        }

        var unreadCount = await db.Notifications
            .AsNoTracking()
            .CountAsync(n => n.RecipientId == userId && !n.IsRead);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1)
            .Include(n => n.Actor)
            .Include(n => n.Post)
            .ToListAsync();

        var hasMore = notifications.Count > pageSize;
        var items = notifications.Take(pageSize).ToList();

        var dtos = items.Select(n => new NotificationDto(
            n.Id,
            n.Type.ToString(),
            n.IsRead,
            n.CreatedAt,
            n.Actor is null ? null : new NotificationActorDto(
                n.Actor.Id,
                n.Actor.Username,
                n.Actor.DisplayName,
                n.Actor.AvatarUrl,
                n.Actor.IsVerified),
            n.PostId,
            n.Post is not null
                ? (n.Post.Content.Length > 80 ? n.Post.Content[..80] + "…" : n.Post.Content)
                : null,
            n.SystemMessage
        )).ToList();

        return new PagedNotificationsDto(dtos, hasMore, unreadCount);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await db.Notifications
            .AsNoTracking()
            .CountAsync(n => n.RecipientId == userId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == userId)
            ?? throw new AppException("Notification not found.", 404);

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await db.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        await db.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }
}
