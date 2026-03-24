namespace samvaad_backend.Models.DTOs.Posts;

public record PostAuthorDto(
    Guid Id,
    string Username,
    string DisplayName,
    string? AvatarUrl,
    bool IsVerified
);
