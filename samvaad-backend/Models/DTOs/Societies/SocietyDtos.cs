namespace samvaad_backend.Models.DTOs.Societies;

public record SocietyDto(
    Guid Id,
    string Name,
    string? Description,
    string? AvatarUrl,
    Guid CreatedByUserId,
    string CreatedByUsername,
    int MembersCount,
    int PostsCount,
    DateTime CreatedAt,
    bool IsMember,
    bool IsAdmin
);

public record SocietyMemberDto(
    Guid UserId,
    string Username,
    string DisplayName,
    string? AvatarUrl,
    string Role,
    DateTime JoinedAt
);

public record CreateSocietyRequest(
    string Name,
    string? Description
);

public record PagedSocietiesDto(
    IReadOnlyList<SocietyDto> Societies,
    bool HasMore,
    Guid? NextCursor
);
