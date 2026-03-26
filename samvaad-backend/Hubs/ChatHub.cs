using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using samvaad_backend.Data;
using samvaad_backend.Models.DTOs.Messages;
using samvaad_backend.Extensions;

namespace samvaad_backend.Hubs;

[Authorize]
public class ChatHub(AppDbContext db, IOnlineTracker onlineTracker) : Hub
{
    // ── Connection lifecycle ─────────────────────────────────────────────────

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User!.GetUserId();
        onlineTracker.SetOnline(userId, Context.ConnectionId);
        Console.WriteLine($"[ChatHub] OnConnectedAsync: userId={userId} connId={Context.ConnectionId}");

        // Join each conversation room the user participates in
        var convIds = await db.ConversationParticipants
            .Where(cp => cp.UserId == userId)
            .Select(cp => cp.ConversationId.ToString())
            .ToListAsync();

        foreach (var convId in convIds)
            await Groups.AddToGroupAsync(Context.ConnectionId, convId);

        // Broadcast presence to all followers and friends of this user
        var followerIds = await db.Follow
            .Where(f => f.FollowingId == userId)
            .Select(f => f.FollowerId.ToString())
            .ToListAsync();

        var friendIds = await db.Friendships
            .Where(f => f.UserId == userId)
            .Select(f => f.FriendId.ToString())
            .ToListAsync();

        foreach (var fid in followerIds.Union(friendIds).Distinct())
            await Clients.User(fid).SendAsync("UserOnline", userId.ToString());

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User!.GetUserId();
        var stillConnected = onlineTracker.SetOffline(userId, Context.ConnectionId);
        Console.WriteLine($"[ChatHub] OnDisconnectedAsync: userId={userId} connId={Context.ConnectionId} stillConnected={stillConnected} exception={exception?.Message}");

        if (!stillConnected)
        {
            // Broadcast offline to followers and friends
            var followerIds = await db.Follow
                .Where(f => f.FollowingId == userId)
                .Select(f => f.FollowerId.ToString())
                .ToListAsync();

            var friendIds = await db.Friendships
                .Where(f => f.UserId == userId)
                .Select(f => f.FriendId.ToString())
                .ToListAsync();

            foreach (var fid in followerIds.Union(friendIds).Distinct())
                await Clients.User(fid).SendAsync("UserOffline", userId.ToString());
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ── Client → Server methods ──────────────────────────────────────────────

    /// <summary>
    /// Called explicitly by the client on logout so the server immediately knows
    /// the user is going offline — without waiting for the WebSocket close handshake.
    /// OnDisconnectedAsync acts as a safety net if this is never called.
    /// </summary>
    public async Task Logout()
    {
        var userId = Context.User!.GetUserId();
        Console.WriteLine($"[ChatHub] Explicit Logout: userId={userId} connId={Context.ConnectionId}");

        var stillConnected = onlineTracker.SetOffline(userId, Context.ConnectionId);

        // Broadcast UserOffline only once (when last connection is gone)
        if (!stillConnected)
        {
            var followerIds = await db.Follow
                .Where(f => f.FollowingId == userId)
                .Select(f => f.FollowerId.ToString())
                .ToListAsync();

            var friendIds = await db.Friendships
                .Where(f => f.UserId == userId)
                .Select(f => f.FriendId.ToString())
                .ToListAsync();

            var targets = followerIds.Union(friendIds).Distinct().ToList();
            Console.WriteLine($"[ChatHub] Broadcasting UserOffline for {userId} to {targets.Count} users");

            foreach (var fid in targets)
                await Clients.User(fid).SendAsync("UserOffline", userId.ToString());
        }
    }

    /// <summary>
    /// Called when a user types in a conversation. Notifies other participants.
    /// </summary>
    public async Task Typing(string conversationId)
    {
        var userId = Context.User!.GetUserId();
        var user = await db.Users.FindAsync(userId);
        if (user == null) return;

        // Verify participant
        var isParticipant = await db.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == Guid.Parse(conversationId) && cp.UserId == userId);
        if (!isParticipant) return;

        await Clients.OthersInGroup(conversationId).SendAsync("UserTyping", new
        {
            ConversationId = conversationId,
            UserId = userId.ToString(),
            DisplayName = user.DisplayName
        });
    }

    /// <summary>
    /// Called when typing stops.
    /// </summary>
    public async Task StopTyping(string conversationId)
    {
        var userId = Context.User!.GetUserId();
        await Clients.OthersInGroup(conversationId).SendAsync("UserStoppedTyping", new
        {
            ConversationId = conversationId,
            UserId = userId.ToString()
        });
    }

    // ── Helpers: join a new conversation room ───────────────────────────────

    public async Task JoinConversation(string conversationId)
    {
        var userId = Context.User!.GetUserId();
        var isParticipant = await db.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == Guid.Parse(conversationId) && cp.UserId == userId);
        if (isParticipant)
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }
}
