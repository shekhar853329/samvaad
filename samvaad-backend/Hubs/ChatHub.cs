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

        // Join each conversation room the user participates in
        var convIds = await db.ConversationParticipants
            .Where(cp => cp.UserId == userId)
            .Select(cp => cp.ConversationId.ToString())
            .ToListAsync();

        foreach (var convId in convIds)
            await Groups.AddToGroupAsync(Context.ConnectionId, convId);

        // Broadcast presence to all followers of this user
        var followerIds = await db.Follow
            .Where(f => f.FollowingId == userId)
            .Select(f => f.FollowerId.ToString())
            .ToListAsync();

        foreach (var fid in followerIds)
            await Clients.User(fid).SendAsync("UserOnline", userId.ToString());

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User!.GetUserId();
        var stillConnected = onlineTracker.SetOffline(userId, Context.ConnectionId);

        if (!stillConnected)
        {
            // Broadcast offline to followers
            var followerIds = await db.Follow
                .Where(f => f.FollowingId == userId)
                .Select(f => f.FollowerId.ToString())
                .ToListAsync();

            foreach (var fid in followerIds)
                await Clients.User(fid).SendAsync("UserOffline", userId.ToString());
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ── Client → Server methods ──────────────────────────────────────────────

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
