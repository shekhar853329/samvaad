namespace samvaad_backend.Models.DTOs.Friends;

/// <summary>Describes the friend-request relationship between the viewer and a profile.</summary>
public enum FriendRequestRelation
{
    /// No request exists either way
    None,
    /// Current user sent a pending request to this profile
    RequestSent,
    /// This profile sent a pending request to the current user
    RequestReceived,
    /// Users are friends (request was accepted)
    Friends
}

public record FriendRequestDto(
    Guid Id,
    Guid SenderId,
    string SenderUsername,
    string SenderDisplayName,
    string? SenderAvatarUrl,
    bool SenderIsVerified,
    DateTime CreatedAt
);
