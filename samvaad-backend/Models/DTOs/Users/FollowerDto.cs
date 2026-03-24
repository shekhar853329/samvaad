namespace samvaad_backend.Models.DTOs.Users;

public record FollowerDto(
    Guid Id,
    string Username,
    string DisplayName,
    string? AvatarUrl,
    bool IsVerified,
    bool IsFollowing
);
