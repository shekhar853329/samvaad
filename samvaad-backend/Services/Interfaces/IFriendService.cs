using samvaad_backend.Models.DTOs.Friends;

namespace samvaad_backend.Services.Interfaces;

public interface IFriendService
{
    /// <summary>Send a friend request from <paramref name="senderId"/> to the user with <paramref name="targetUsername"/>.</summary>
    Task SendRequestAsync(Guid senderId, string targetUsername);

    /// <summary>Cancel a pending request that <paramref name="senderId"/> previously sent.</summary>
    Task CancelRequestAsync(Guid senderId, string targetUsername);

    /// <summary>Accept a pending friend request sent to <paramref name="receiverId"/> by the user with <paramref name="senderUsername"/>.</summary>
    Task AcceptRequestAsync(Guid receiverId, string senderUsername);

    /// <summary>Decline a pending friend request sent to <paramref name="receiverId"/> by the user with <paramref name="senderUsername"/>.</summary>
    Task DeclineRequestAsync(Guid receiverId, string senderUsername);

    /// <summary>Remove an existing friendship (either user can unfriend).</summary>
    Task UnfriendAsync(Guid actorId, string otherUsername);

    /// <summary>Get all pending requests received by <paramref name="userId"/> (inbox).</summary>
    Task<IReadOnlyList<FriendRequestDto>> GetPendingRequestsAsync(Guid userId);

    /// <summary>Get the friend-request relation between <paramref name="viewerId"/> and the user identified by <paramref name="targetUserId"/>.</summary>
    Task<FriendRequestRelation> GetRelationAsync(Guid viewerId, Guid targetUserId);

    /// <summary>Get the list of friends for <paramref name="userId"/>.</summary>
    Task<IReadOnlyList<FriendRequestDto>> GetFriendsAsync(Guid userId, int page, int pageSize);

    /// <summary>Get friends of a user identified by username (public endpoint).</summary>
    Task<IReadOnlyList<FriendRequestDto>> GetFriendsByUsernameAsync(string username, int page, int pageSize);

    /// <summary>Get the total number of friends for <paramref name="userId"/>.</summary>
    Task<int> GetFriendsCountAsync(Guid userId);

    /// <summary>Get the set of user IDs that are friends with <paramref name="userId"/>.</summary>
    Task<IReadOnlySet<Guid>> GetFriendIdsAsync(Guid userId);
}
