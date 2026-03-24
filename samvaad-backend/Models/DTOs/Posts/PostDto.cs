namespace samvaad_backend.Models.DTOs.Posts;

public record PostDto(
    Guid Id,
    PostAuthorDto Author,
    string Content,
    DateTime CreatedAt,
    int LikesCount,
    int CommentsCount,
    int RepostsCount,
    int ViewsCount,
    bool IsLiked,
    bool IsBookmarked,
    bool IsReposted,
    bool IsOwnPost,
    IReadOnlyList<PostMediaDto> Media,
    IReadOnlyList<string> Hashtags,
    Guid? ParentPostId,
    Guid? RepostOfId
);
