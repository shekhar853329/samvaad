using Microsoft.EntityFrameworkCore;
using samvaad_backend.Common;
using samvaad_backend.Data;
using samvaad_backend.Models.DTOs.Explore;
using samvaad_backend.Models.DTOs.Posts;
using samvaad_backend.Models.DTOs.Users;
using samvaad_backend.Models.Entities;
using samvaad_backend.Services.Interfaces;

namespace samvaad_backend.Services;

public class ExploreService(AppDbContext db) : IExploreService
{
    public async Task<List<TrendingHashtagDto>> GetTrendingHashtagsAsync(int count = 10)
    {
        return await db.HashTags
            .AsNoTracking()
            .OrderByDescending(h => h.PostCount)
            .Take(count)
            .Select(h => new TrendingHashtagDto(h.Id, h.Name, h.Category, h.PostCount))
            .ToListAsync();
    }

    public async Task<PagedPostsDto> GetTrendingPostsAsync(Guid? requestingUserId, int page = 1, int pageSize = 20)
    {
        var since = DateTime.UtcNow.AddDays(-7);

        var posts = await db.Posts
            .AsNoTracking()
            .Where(p => p.ParentPostId == null && p.CreatedAt >= since)
            .OrderByDescending(p => p.LikesCount + p.CommentsCount + p.RepostsCount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1)
            .Include(p => p.Author)
            .Include(p => p.Media.OrderBy(m => m.DisplayOrder))
            .Include(p => p.PostHashTags).ThenInclude(ph => ph.HashTag)
            .ToListAsync();

        var hasMore = posts.Count > pageSize;
        var items = posts.Take(pageSize).ToList();
        var dtos = await MapManyToDtoAsync(items, requestingUserId);
        return new PagedPostsDto(dtos, hasMore, hasMore ? items.LastOrDefault()?.Id : null);
    }

    public async Task<PagedPostsDto> GetPostsByHashtagAsync(string tag, Guid? requestingUserId, int page = 1, int pageSize = 20)
    {
        var normalizedTag = tag.ToLowerInvariant().TrimStart('#');

        var posts = await db.Posts
            .AsNoTracking()
            .Where(p => p.PostHashTags.Any(ph => ph.HashTag.Name == normalizedTag))
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1)
            .Include(p => p.Author)
            .Include(p => p.Media.OrderBy(m => m.DisplayOrder))
            .Include(p => p.PostHashTags).ThenInclude(ph => ph.HashTag)
            .ToListAsync();

        var hasMore = posts.Count > pageSize;
        var items = posts.Take(pageSize).ToList();
        var dtos = await MapManyToDtoAsync(items, requestingUserId);
        return new PagedPostsDto(dtos, hasMore, hasMore ? items.LastOrDefault()?.Id : null);
    }

    public async Task<PagedPostsDto> SearchPostsAsync(string query, Guid? requestingUserId, int page = 1, int pageSize = 20)
    {
        var lower = query.ToLowerInvariant();

        var posts = await db.Posts
            .AsNoTracking()
            .Where(p => EF.Functions.ILike(p.Content, $"%{lower}%")
                     || p.PostHashTags.Any(ph => EF.Functions.ILike(ph.HashTag.Name, $"%{lower}%")))
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1)
            .Include(p => p.Author)
            .Include(p => p.Media.OrderBy(m => m.DisplayOrder))
            .Include(p => p.PostHashTags).ThenInclude(ph => ph.HashTag)
            .ToListAsync();

        var hasMore = posts.Count > pageSize;
        var items = posts.Take(pageSize).ToList();
        var dtos = await MapManyToDtoAsync(items, requestingUserId);
        return new PagedPostsDto(dtos, hasMore, hasMore ? items.LastOrDefault()?.Id : null);
    }

    public async Task<PagedUsersDto> SearchUsersAsync(string query, Guid? requestingUserId, int page = 1, int pageSize = 20)
    {
        var users = await db.Users
            .AsNoTracking()
            .Where(u => EF.Functions.ILike(u.Username, $"%{query}%")
                     || EF.Functions.ILike(u.DisplayName, $"%{query}%"))
            .OrderByDescending(u => u.FollowersCount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1)
            .ToListAsync();

        var hasMore = users.Count > pageSize;
        var items = users.Take(pageSize).ToList();

        HashSet<Guid> followingIds = [];
        if (requestingUserId.HasValue)
        {
            var userIds = items.Select(u => u.Id).ToList();
            followingIds = await db.Follow
                .AsNoTracking()
                .Where(f => f.FollowerId == requestingUserId.Value && userIds.Contains(f.FollowingId))
                .Select(f => f.FollowingId)
                .ToHashSetAsync();
        }

        var dtos = items.Select(u => new FollowerDto(
            u.Id, u.Username, u.DisplayName, u.AvatarUrl, u.IsVerified,
            followingIds.Contains(u.Id)
        )).ToList();

        return new PagedUsersDto(dtos, hasMore);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<IReadOnlyList<PostDto>> MapManyToDtoAsync(IList<Post> posts, Guid? requestingUserId)
    {
        if (!requestingUserId.HasValue || posts.Count == 0)
            return posts.Select(p => BuildDto(p, null, false, false, false, false)).ToList();

        var uid = requestingUserId.Value;
        var postIds = posts.Select(p => p.Id).ToList();
        var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();

        var likedIds = await db.Likes.AsNoTracking()
            .Where(l => l.UserId == uid && postIds.Contains(l.PostId))
            .Select(l => l.PostId).ToHashSetAsync();

        var bookmarkedIds = await db.Bookmarks.AsNoTracking()
            .Where(b => b.UserId == uid && postIds.Contains(b.PostId))
            .Select(b => b.PostId).ToHashSetAsync();

        var repostedOriginalIds = await db.Posts.AsNoTracking()
            .Where(p => p.AuthorId == uid && p.RepostOfId != null && postIds.Contains(p.RepostOfId!.Value))
            .Select(p => p.RepostOfId!.Value).ToHashSetAsync();

        var followedAuthorIds = await db.Follow.AsNoTracking()
            .Where(f => f.FollowerId == uid && authorIds.Contains(f.FollowingId))
            .Select(f => f.FollowingId).ToHashSetAsync();

        return posts.Select(p => BuildDto(
            p, requestingUserId,
            likedIds.Contains(p.Id),
            bookmarkedIds.Contains(p.Id),
            repostedOriginalIds.Contains(p.Id),
            followedAuthorIds.Contains(p.AuthorId) && p.AuthorId != uid
        )).ToList();
    }

    private static PostDto BuildDto(Post p, Guid? requestingUserId, bool isLiked, bool isBookmarked, bool isReposted, bool isFollowingAuthor) =>
        new(
            p.Id,
            new PostAuthorDto(p.Author.Id, p.Author.Username, p.Author.DisplayName, p.Author.AvatarUrl, p.Author.IsVerified),
            p.Content,
            p.CreatedAt,
            p.LikesCount,
            p.CommentsCount,
            p.RepostsCount,
            p.ViewsCount,
            isLiked,
            isBookmarked,
            isReposted,
            requestingUserId.HasValue && p.AuthorId == requestingUserId.Value,
            p.Media.OrderBy(m => m.DisplayOrder)
                   .Select(m => new PostMediaDto(m.Id, m.Url, m.MediaType.ToString(), m.Width, m.Height, m.DisplayOrder))
                   .ToList(),
            p.PostHashTags.Select(ph => ph.HashTag.Name).ToList(),
            p.ParentPostId,
            p.RepostOfId,
            isFollowingAuthor
        );
}
