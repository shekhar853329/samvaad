using Microsoft.EntityFrameworkCore;
using samvaad_backend.Common;
using samvaad_backend.Data;
using samvaad_backend.Models.DTOs.Friends;
using samvaad_backend.Models.Entities;
using samvaad_backend.Models.Enums;
using samvaad_backend.Services.Interfaces;

namespace samvaad_backend.Services;

public class FriendService(AppDbContext db, INotificationService notificationService) : IFriendService
{
    public async Task SendRequestAsync(Guid senderId, string targetUsername)
    {
        var receiver = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == targetUsername)
            ?? throw new AppException("User not found.", 404);

        if (receiver.Id == senderId)
            throw new AppException("You cannot send a friend request to yourself.", 400);

        // Fast check: are they already friends? (single-column Friendships lookup)
        var alreadyFriends = await db.Friendships
            .AnyAsync(f => f.UserId == senderId && f.FriendId == receiver.Id);
        if (alreadyFriends)
            throw new AppException("You are already friends.", 409);

        // Reject if a pending request already exists in either direction
        var existing = await db.FriendRequests
            .FirstOrDefaultAsync(fr =>
                fr.Status == FriendRequestStatus.Pending &&
                ((fr.SenderId == senderId && fr.ReceiverId == receiver.Id) ||
                 (fr.SenderId == receiver.Id && fr.ReceiverId == senderId)));

        if (existing is not null)
            throw new AppException("A friend request already exists.", 409);

        db.FriendRequests.Add(new FriendRequest
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            ReceiverId = receiver.Id,
            Status = FriendRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        await notificationService.CreateAsync(receiver.Id, senderId, NotificationType.FriendRequest);
    }

    public async Task CancelRequestAsync(Guid senderId, string targetUsername)
    {
        var receiver = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == targetUsername)
            ?? throw new AppException("User not found.", 404);

        var request = await db.FriendRequests
            .FirstOrDefaultAsync(fr => fr.SenderId == senderId && fr.ReceiverId == receiver.Id && fr.Status == FriendRequestStatus.Pending)
            ?? throw new AppException("No pending request found.", 404);

        request.Status = FriendRequestStatus.Cancelled;
        request.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task AcceptRequestAsync(Guid receiverId, string senderUsername)
    {
        var sender = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == senderUsername)
            ?? throw new AppException("User not found.", 404);

        var request = await db.FriendRequests
            .FirstOrDefaultAsync(fr => fr.SenderId == sender.Id && fr.ReceiverId == receiverId && fr.Status == FriendRequestStatus.Pending)
            ?? throw new AppException("No pending request found.", 404);

        // Update the FriendRequest record to Accepted (kept for history)
        request.Status = FriendRequestStatus.Accepted;
        request.UpdatedAt = DateTime.UtcNow;

        // Insert two rows into the Friendships adjacency list — single-column indexed lookups
        db.Friendships.Add(new Friendship { UserId = receiverId,  FriendId = sender.Id,  CreatedAt = DateTime.UtcNow });
        db.Friendships.Add(new Friendship { UserId = sender.Id,   FriendId = receiverId, CreatedAt = DateTime.UtcNow });

        await db.SaveChangesAsync();
        await notificationService.CreateAsync(sender.Id, receiverId, NotificationType.FriendRequestAccepted);
    }

    public async Task DeclineRequestAsync(Guid receiverId, string senderUsername)
    {
        var sender = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == senderUsername)
            ?? throw new AppException("User not found.", 404);

        var request = await db.FriendRequests
            .FirstOrDefaultAsync(fr => fr.SenderId == sender.Id && fr.ReceiverId == receiverId && fr.Status == FriendRequestStatus.Pending)
            ?? throw new AppException("No pending request found.", 404);

        request.Status = FriendRequestStatus.Declined;
        request.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task UnfriendAsync(Guid actorId, string otherUsername)
    {
        var other = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == otherUsername)
            ?? throw new AppException("User not found.", 404);

        // Single-column lookup on the Friendships table (indexed on UserId)
        var myRow = await db.Friendships
            .FirstOrDefaultAsync(f => f.UserId == actorId && f.FriendId == other.Id)
            ?? throw new AppException("You are not friends with this user.", 404);

        var theirRow = await db.Friendships
            .FirstOrDefaultAsync(f => f.UserId == other.Id && f.FriendId == actorId);

        db.Friendships.Remove(myRow);
        if (theirRow is not null) db.Friendships.Remove(theirRow);

        // Also mark the original FriendRequest record (if any) for consistency
        var friendReq = await db.FriendRequests.FirstOrDefaultAsync(fr =>
            fr.Status == FriendRequestStatus.Accepted &&
            ((fr.SenderId == actorId && fr.ReceiverId == other.Id) ||
             (fr.SenderId == other.Id && fr.ReceiverId == actorId)));
        if (friendReq is not null) db.FriendRequests.Remove(friendReq);

        await db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<FriendRequestDto>> GetPendingRequestsAsync(Guid userId)
    {
        return await db.FriendRequests
            .AsNoTracking()
            .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
            .OrderByDescending(fr => fr.CreatedAt)
            .Include(fr => fr.Sender)
            .Select(fr => new FriendRequestDto(
                fr.Id,
                fr.SenderId,
                fr.Sender.Username,
                fr.Sender.DisplayName,
                fr.Sender.AvatarUrl,
                fr.Sender.IsVerified,
                fr.CreatedAt))
            .ToListAsync();
    }

    public async Task<FriendRequestRelation> GetRelationAsync(Guid viewerId, Guid targetUserId)
    {
        if (viewerId == targetUserId) return FriendRequestRelation.None;

        // Fast single-column lookup on the Friendships adjacency list
        var areFriends = await db.Friendships
            .AsNoTracking()
            .AnyAsync(f => f.UserId == viewerId && f.FriendId == targetUserId);

        if (areFriends) return FriendRequestRelation.Friends;

        // Fall back to FriendRequests for pending states
        var request = await db.FriendRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(fr =>
                fr.Status == FriendRequestStatus.Pending &&
                ((fr.SenderId == viewerId && fr.ReceiverId == targetUserId) ||
                 (fr.SenderId == targetUserId && fr.ReceiverId == viewerId)));

        return request switch
        {
            null => FriendRequestRelation.None,
            { } r when r.SenderId == viewerId => FriendRequestRelation.RequestSent,
            _ => FriendRequestRelation.RequestReceived
        };
    }

    public async Task<IReadOnlyList<FriendRequestDto>> GetFriendsAsync(Guid userId, int page, int pageSize)
    {
        return await db.Friendships
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(f => f.Friend)
            .Select(f => new FriendRequestDto(
                f.FriendId,
                f.FriendId,
                f.Friend.Username,
                f.Friend.DisplayName,
                f.Friend.AvatarUrl,
                f.Friend.IsVerified,
                f.CreatedAt))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<FriendRequestDto>> GetFriendsByUsernameAsync(string username, int page, int pageSize)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username)
            ?? throw new AppException("User not found.", 404);

        return await GetFriendsAsync(user.Id, page, pageSize);
    }

    public async Task<int> GetFriendsCountAsync(Guid userId)
    {
        return await db.Friendships.CountAsync(f => f.UserId == userId);
    }
}
