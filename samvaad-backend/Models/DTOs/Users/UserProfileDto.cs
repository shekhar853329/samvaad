namespace samvaad_backend.Models.DTOs.Users;

public record UserProfileDto(
    Guid Id,
    string Username,
    string DisplayName,
    string? AvatarUrl,
    string? CoverImageUrl,
    string? Bio,
    string? Location,
    string? Website,
    bool IsVerified,
    bool IsPrivate,
    int PostsCount,
    int FollowersCount,
    int FollowingCount,
    long TotalViewsCount,
    DateTime JoinedAt,
    bool IsFollowing,
    bool IsOwnProfile
);
