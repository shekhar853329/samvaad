using samvaad_backend.Models.DTOs.Explore;
using samvaad_backend.Models.DTOs.Posts;

namespace samvaad_backend.Services.Interfaces;

public interface IExploreService
{
    Task<List<TrendingHashtagDto>> GetTrendingHashtagsAsync(int count = 10);
    Task<PagedPostsDto> GetTrendingPostsAsync(Guid? requestingUserId, int page = 1, int pageSize = 20);
    Task<PagedPostsDto> GetPostsByHashtagAsync(string tag, Guid? requestingUserId, int page = 1, int pageSize = 20);
    Task<PagedPostsDto> SearchPostsAsync(string query, Guid? requestingUserId, int page = 1, int pageSize = 20);
    Task<PagedUsersDto> SearchUsersAsync(string query, Guid? requestingUserId, int page = 1, int pageSize = 20);
}
