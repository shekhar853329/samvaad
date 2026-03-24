using samvaad_backend.Models.DTOs.Posts;

namespace samvaad_backend.Services.Interfaces;

public interface IPostService
{
    Task<PostDto> CreateAsync(Guid authorId, CreatePostRequest request);
    Task<PostDto> GetByIdAsync(Guid postId, Guid? requestingUserId);
    Task DeleteAsync(Guid postId, Guid requestingUserId);
    Task<PostDto> LikeAsync(Guid postId, Guid userId);
    Task<PostDto> UnlikeAsync(Guid postId, Guid userId);
    Task<PostDto> BookmarkAsync(Guid postId, Guid userId);
    Task<PostDto> UnbookmarkAsync(Guid postId, Guid userId);
    Task<PostDto> RepostAsync(Guid postId, Guid userId);
    Task<PostDto> UnrepostAsync(Guid postId, Guid userId);
    Task<PagedPostsDto> GetRepliesAsync(Guid postId, Guid? requestingUserId, int page, int pageSize);
    Task<PagedPostsDto> GetUserPostsAsync(string username, Guid? requestingUserId, int page, int pageSize);
    Task<PagedPostsDto> GetFeedAsync(Guid userId, int page, int pageSize);
}
