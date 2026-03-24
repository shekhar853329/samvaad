using Microsoft.EntityFrameworkCore;
using samvaad_backend.Common;
using samvaad_backend.Data;
using samvaad_backend.Models.DTOs.Posts;
using samvaad_backend.Models.Entities;
using samvaad_backend.Services.Interfaces;
using System.Text.RegularExpressions;

namespace samvaad_backend.Services;

public partial class PostService(AppDbContext db) : IPostService
{
    [GeneratedRegex(@"#(\w+)", RegexOptions.Compiled)]
    private static partial Regex HashtagRegex();

    public async Task<PostDto> CreateAsync(Guid authorId, CreatePostRequest request)
    {
        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = authorId,
            Content = request.Content,
            ParentPostId = request.ParentPostId,
            RepostOfId = request.RepostOfId,
            CreatedAt = DateTime.UtcNow
        };

        // Extract and upsert hashtags
        var tagNames = HashtagRegex().Matches(request.Content)
            .Select(m => m.Groups[1].Value.ToLowerInvariant())
            .Distinct()
            .ToList();

        foreach (var name in tagNames)
        {
            var tag = await db.HashTags.FirstOrDefaultAsync(h => h.Name == name);
            if (tag is null)
            {
                tag = new HashTag { Id = Guid.NewGuid(), Name = name, PostCount = 1 };
                db.HashTags.Add(tag);
            }
            else
            {
                await db.HashTags
                    .Where(h => h.Id == tag.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(h => h.PostCount, h => h.PostCount + 1));
            }
            post.PostHashTags.Add(new PostHashTag { Post = post, HashTag = tag });
        }

        // Increment parent reply count
        if (request.ParentPostId.HasValue)
        {
            await db.Posts
                .Where(p => p.Id == request.ParentPostId.Value)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.CommentsCount, p => p.CommentsCount + 1));
        }

        // Increment repost count on original
        if (request.RepostOfId.HasValue)
        {
            await db.Posts
                .Where(p => p.Id == request.RepostOfId.Value)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.RepostsCount, p => p.RepostsCount + 1));
        }

        // Increment author post count
        await db.Users
            .Where(u => u.Id == authorId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.PostsCount, u => u.PostsCount + 1));

        db.Posts.Add(post);
        await db.SaveChangesAsync();

        return await GetByIdAsync(post.Id, authorId);
    }

    public async Task<PostDto> GetByIdAsync(Guid postId, Guid? requestingUserId)
    {
        var post = await db.Posts
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.Media.OrderBy(m => m.DisplayOrder))
            .Include(p => p.PostHashTags).ThenInclude(ph => ph.HashTag)
            .FirstOrDefaultAsync(p => p.Id == postId)
            ?? throw new AppException("Post not found.", 404);

        return await MapToDtoAsync(post, requestingUserId);
    }

    public async Task DeleteAsync(Guid postId, Guid requestingUserId)
    {
        var post = await db.Posts.FindAsync(postId)
            ?? throw new AppException("Post not found.", 404);

        if (post.AuthorId != requestingUserId)
            throw new AppException("You can only delete your own posts.", 403);

        // Decrement parent reply count
        if (post.ParentPostId.HasValue)
        {
            await db.Posts
                .Where(p => p.Id == post.ParentPostId.Value)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.CommentsCount, p => p.CommentsCount - 1));
        }

        await db.Users
            .Where(u => u.Id == requestingUserId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.PostsCount, u => u.PostsCount - 1));

        db.Posts.Remove(post);
        await db.SaveChangesAsync();
    }

    public async Task<PostDto> LikeAsync(Guid postId, Guid userId)
    {
        var post = await db.Posts.FindAsync(postId)
            ?? throw new AppException("Post not found.", 404);

        var exists = await db.Likes.AnyAsync(l => l.PostId == postId && l.UserId == userId);
        if (exists) throw new AppException("You already liked this post.", 409);

        db.Likes.Add(new Like { PostId = postId, UserId = userId, CreatedAt = DateTime.UtcNow });
        await db.Posts
            .Where(p => p.Id == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.LikesCount, p => p.LikesCount + 1));

        await db.SaveChangesAsync();
        return await GetByIdAsync(postId, userId);
    }

    public async Task<PostDto> UnlikeAsync(Guid postId, Guid userId)
    {
        var like = await db.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId)
            ?? throw new AppException("You have not liked this post.", 404);

        db.Likes.Remove(like);
        await db.Posts
            .Where(p => p.Id == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.LikesCount, p => p.LikesCount - 1));

        await db.SaveChangesAsync();
        return await GetByIdAsync(postId, userId);
    }

    public async Task<PostDto> BookmarkAsync(Guid postId, Guid userId)
    {
        _ = await db.Posts.FindAsync(postId) ?? throw new AppException("Post not found.", 404);

        var exists = await db.Bookmarks.AnyAsync(b => b.PostId == postId && b.UserId == userId);
        if (exists) throw new AppException("Already bookmarked.", 409);

        db.Bookmarks.Add(new Bookmark { PostId = postId, UserId = userId, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return await GetByIdAsync(postId, userId);
    }

    public async Task<PostDto> UnbookmarkAsync(Guid postId, Guid userId)
    {
        var bookmark = await db.Bookmarks.FirstOrDefaultAsync(b => b.PostId == postId && b.UserId == userId)
            ?? throw new AppException("Bookmark not found.", 404);

        db.Bookmarks.Remove(bookmark);
        await db.SaveChangesAsync();
        return await GetByIdAsync(postId, userId);
    }

    public async Task<PostDto> RepostAsync(Guid originalPostId, Guid userId)
    {
        _ = await db.Posts.FindAsync(originalPostId) ?? throw new AppException("Post not found.", 404);

        var alreadyReposted = await db.Posts.AnyAsync(p =>
            p.AuthorId == userId && p.RepostOfId == originalPostId);
        if (alreadyReposted) throw new AppException("Already reposted.", 409);

        return await CreateAsync(userId, new CreatePostRequest
        {
            Content = string.Empty,   // repost has no new content
            RepostOfId = originalPostId
        });
    }

    public async Task<PostDto> UnrepostAsync(Guid originalPostId, Guid userId)
    {
        var repost = await db.Posts
            .FirstOrDefaultAsync(p => p.AuthorId == userId && p.RepostOfId == originalPostId)
            ?? throw new AppException("Repost not found.", 404);

        await DeleteAsync(repost.Id, userId);
        return await GetByIdAsync(originalPostId, userId);
    }

    public async Task<PagedPostsDto> GetRepliesAsync(Guid postId, Guid? requestingUserId, int page, int pageSize)
    {
        var posts = await db.Posts
            .AsNoTracking()
            .Where(p => p.ParentPostId == postId)
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

    public async Task<PagedPostsDto> GetUserPostsAsync(string username, Guid? requestingUserId, int page, int pageSize)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username.ToLowerInvariant())
            ?? throw new AppException($"User '{username}' not found.", 404);

        var posts = await db.Posts
            .AsNoTracking()
            .Where(p => p.AuthorId == user.Id && p.ParentPostId == null)
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

    public async Task<PagedPostsDto> GetFeedAsync(Guid userId, int page, int pageSize)
    {
        // Posts from users the current user follows + own posts
        var followingIds = await db.Follow
            .AsNoTracking()
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        followingIds.Add(userId);

        var posts = await db.Posts
            .AsNoTracking()
            .Where(p => followingIds.Contains(p.AuthorId) && p.ParentPostId == null)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1)
            .Include(p => p.Author)
            .Include(p => p.Media.OrderBy(m => m.DisplayOrder))
            .Include(p => p.PostHashTags).ThenInclude(ph => ph.HashTag)
            .ToListAsync();

        var hasMore = posts.Count > pageSize;
        var items = posts.Take(pageSize).ToList();

        var dtos = await MapManyToDtoAsync(items, userId);
        return new PagedPostsDto(dtos, hasMore, hasMore ? items.LastOrDefault()?.Id : null);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<PostDto> MapToDtoAsync(Post post, Guid? requestingUserId)
    {
        bool isLiked = false, isBookmarked = false, isReposted = false;

        if (requestingUserId.HasValue)
        {
            var uid = requestingUserId.Value;
            isLiked = await db.Likes.AnyAsync(l => l.PostId == post.Id && l.UserId == uid);
            isBookmarked = await db.Bookmarks.AnyAsync(b => b.PostId == post.Id && b.UserId == uid);
            isReposted = await db.Posts.AnyAsync(p => p.AuthorId == uid && p.RepostOfId == post.Id);
        }

        return BuildDto(post, requestingUserId, isLiked, isBookmarked, isReposted);
    }

    private async Task<IReadOnlyList<PostDto>> MapManyToDtoAsync(IList<Post> posts, Guid? requestingUserId)
    {
        if (!requestingUserId.HasValue || posts.Count == 0)
            return posts.Select(p => BuildDto(p, null, false, false, false)).ToList();

        var uid = requestingUserId.Value;
        var postIds = posts.Select(p => p.Id).ToList();

        var likedIds = await db.Likes.AsNoTracking()
            .Where(l => l.UserId == uid && postIds.Contains(l.PostId))
            .Select(l => l.PostId).ToHashSetAsync();

        var bookmarkedIds = await db.Bookmarks.AsNoTracking()
            .Where(b => b.UserId == uid && postIds.Contains(b.PostId))
            .Select(b => b.PostId).ToHashSetAsync();

        var repostedOriginalIds = await db.Posts.AsNoTracking()
            .Where(p => p.AuthorId == uid && p.RepostOfId != null && postIds.Contains(p.RepostOfId!.Value))
            .Select(p => p.RepostOfId!.Value).ToHashSetAsync();

        return posts.Select(p => BuildDto(
            p, requestingUserId,
            likedIds.Contains(p.Id),
            bookmarkedIds.Contains(p.Id),
            repostedOriginalIds.Contains(p.Id)
        )).ToList();
    }

    private static PostDto BuildDto(Post p, Guid? requestingUserId, bool isLiked, bool isBookmarked, bool isReposted) =>
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
            p.RepostOfId
        );
}
