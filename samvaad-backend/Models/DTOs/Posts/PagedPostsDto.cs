namespace samvaad_backend.Models.DTOs.Posts;

public record PagedPostsDto(
    IReadOnlyList<PostDto> Posts,
    bool HasMore,
    Guid? NextCursor
);
