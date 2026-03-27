using Microsoft.EntityFrameworkCore;
using samvaad_backend.Common;
using samvaad_backend.Data;
using samvaad_backend.Models.DTOs.Posts;
using samvaad_backend.Models.DTOs.Societies;
using samvaad_backend.Models.Entities;
using samvaad_backend.Services.Interfaces;

namespace samvaad_backend.Services;

public class SocietyService(AppDbContext db) : ISocietyService
{
    // ── Create ─────────────────────────────────────────────────────────────────
    public async Task<SocietyDto> CreateAsync(Guid userId, CreateSocietyRequest request)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new AppException("Society name is required.", 400);

        var exists = await db.Societies.AnyAsync(s => s.Name == name);
        if (exists)
            throw new AppException($"A society named '{name}' already exists.", 409);

        var society = new Society
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = request.Description?.Trim(),
            CreatedByUserId = userId,
            MembersCount = 1,
            PostsCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        var adminMember = new SocietyMember
        {
            SocietyId = society.Id,
            UserId = userId,
            Role = SocietyRole.Admin,
            JoinedAt = society.CreatedAt
        };

        db.Societies.Add(society);
        db.SocietyMembers.Add(adminMember);
        await db.SaveChangesAsync();

        return await GetByIdAsync(society.Id, userId);
    }

    // ── GetById ────────────────────────────────────────────────────────────────
    public async Task<SocietyDto> GetByIdAsync(Guid societyId, Guid requestingUserId)
    {
        var society = await db.Societies
            .AsNoTracking()
            .Include(s => s.CreatedBy)
            .FirstOrDefaultAsync(s => s.Id == societyId)
            ?? throw new AppException("Society not found.", 404);

        var membership = await db.SocietyMembers.AsNoTracking()
            .FirstOrDefaultAsync(sm => sm.SocietyId == societyId && sm.UserId == requestingUserId);

        return ToDto(society, membership);
    }

    // ── GetAll ─────────────────────────────────────────────────────────────────
    public async Task<PagedSocietiesDto> GetAllAsync(Guid requestingUserId, int page, int pageSize)
    {
        var societies = await db.Societies
            .AsNoTracking()
            .Include(s => s.CreatedBy)
            .OrderByDescending(s => s.MembersCount)
            .ThenByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1)
            .ToListAsync();

        var hasMore = societies.Count > pageSize;
        var items = societies.Take(pageSize).ToList();

        var societyIds = items.Select(s => s.Id).ToList();
        var memberships = await db.SocietyMembers.AsNoTracking()
            .Where(sm => sm.UserId == requestingUserId && societyIds.Contains(sm.SocietyId))
            .ToDictionaryAsync(sm => sm.SocietyId);

        var dtos = items.Select(s => ToDto(s, memberships.GetValueOrDefault(s.Id))).ToList();
        return new PagedSocietiesDto(dtos, hasMore, hasMore ? items.LastOrDefault()?.Id : null);
    }

    // ── GetUserSocieties ───────────────────────────────────────────────────────
    public async Task<PagedSocietiesDto> GetUserSocietiesAsync(Guid userId, int page, int pageSize)
    {
        var memberSocietyIds = await db.SocietyMembers.AsNoTracking()
            .Where(sm => sm.UserId == userId)
            .Select(sm => sm.SocietyId)
            .ToListAsync();

        var societies = await db.Societies
            .AsNoTracking()
            .Include(s => s.CreatedBy)
            .Where(s => memberSocietyIds.Contains(s.Id))
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1)
            .ToListAsync();

        var hasMore = societies.Count > pageSize;
        var items = societies.Take(pageSize).ToList();

        var societyIds = items.Select(s => s.Id).ToList();
        var memberships = await db.SocietyMembers.AsNoTracking()
            .Where(sm => sm.UserId == userId && societyIds.Contains(sm.SocietyId))
            .ToDictionaryAsync(sm => sm.SocietyId);

        var dtos = items.Select(s => ToDto(s, memberships.GetValueOrDefault(s.Id))).ToList();
        return new PagedSocietiesDto(dtos, hasMore, hasMore ? items.LastOrDefault()?.Id : null);
    }

    // ── Join ───────────────────────────────────────────────────────────────────
    public async Task<SocietyDto> JoinAsync(Guid societyId, Guid userId)
    {
        _ = await db.Societies.FindAsync(societyId)
            ?? throw new AppException("Society not found.", 404);

        var exists = await db.SocietyMembers
            .AnyAsync(sm => sm.SocietyId == societyId && sm.UserId == userId);
        if (exists) throw new AppException("You are already a member of this society.", 409);

        db.SocietyMembers.Add(new SocietyMember
        {
            SocietyId = societyId,
            UserId = userId,
            Role = SocietyRole.Member,
            JoinedAt = DateTime.UtcNow
        });

        await db.Societies
            .Where(s => s.Id == societyId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.MembersCount, x => x.MembersCount + 1));

        await db.SaveChangesAsync();
        return await GetByIdAsync(societyId, userId);
    }

    // ── Leave ──────────────────────────────────────────────────────────────────
    public async Task LeaveAsync(Guid societyId, Guid userId)
    {
        var member = await db.SocietyMembers
            .FirstOrDefaultAsync(sm => sm.SocietyId == societyId && sm.UserId == userId)
            ?? throw new AppException("You are not a member of this society.", 404);

        if (member.Role == SocietyRole.Admin)
        {
            // Make sure another admin exists before leaving, or just allow leaving (society persists)
            var otherAdminExists = await db.SocietyMembers
                .AnyAsync(sm => sm.SocietyId == societyId && sm.UserId != userId && sm.Role == SocietyRole.Admin);
            if (!otherAdminExists)
                throw new AppException("You are the only admin. Transfer admin rights before leaving.", 400);
        }

        db.SocietyMembers.Remove(member);

        await db.Societies
            .Where(s => s.Id == societyId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.MembersCount, x => x.MembersCount - 1));

        await db.SaveChangesAsync();
    }

    // ── GetMembers ─────────────────────────────────────────────────────────────
    public async Task<IReadOnlyList<SocietyMemberDto>> GetMembersAsync(Guid societyId, Guid requestingUserId)
    {
        var isMember = await db.SocietyMembers
            .AnyAsync(sm => sm.SocietyId == societyId && sm.UserId == requestingUserId);
        if (!isMember) throw new AppException("You are not a member of this society.", 403);

        return await db.SocietyMembers
            .AsNoTracking()
            .Where(sm => sm.SocietyId == societyId)
            .Include(sm => sm.User)
            .OrderBy(sm => sm.JoinedAt)
            .Select(sm => new SocietyMemberDto(
                sm.UserId,
                sm.User.Username,
                sm.User.DisplayName,
                sm.User.AvatarUrl,
                sm.Role.ToString(),
                sm.JoinedAt))
            .ToListAsync();
    }

    // ── GetFeed ────────────────────────────────────────────────────────────────
    public async Task<PagedPostsDto> GetFeedAsync(Guid societyId, Guid requestingUserId, int page, int pageSize)
    {
        var isMember = await db.SocietyMembers
            .AnyAsync(sm => sm.SocietyId == societyId && sm.UserId == requestingUserId);
        if (!isMember) throw new AppException("You are not a member of this society.", 403);

        var posts = await db.Posts
            .AsNoTracking()
            .Where(p => p.SocietyId == societyId && p.ParentPostId == null)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1)
            .Include(p => p.Author)
            .Include(p => p.Media.OrderBy(m => m.DisplayOrder))
            .Include(p => p.PostHashTags).ThenInclude(ph => ph.HashTag)
            .ToListAsync();

        var hasMore = posts.Count > pageSize;
        var items = posts.Take(pageSize).ToList();

        var postIds = items.Select(p => p.Id).ToList();
        var uid = requestingUserId;

        var likedIds = await db.Likes.AsNoTracking()
            .Where(l => l.UserId == uid && postIds.Contains(l.PostId))
            .Select(l => l.PostId).ToHashSetAsync();

        var bookmarkedIds = await db.Bookmarks.AsNoTracking()
            .Where(b => b.UserId == uid && postIds.Contains(b.PostId))
            .Select(b => b.PostId).ToHashSetAsync();

        var dtos = items.Select(p => BuildPostDto(p, uid, likedIds.Contains(p.Id), bookmarkedIds.Contains(p.Id))).ToList();
        return new PagedPostsDto(dtos, hasMore, hasMore ? items.LastOrDefault()?.Id : null);
    }

    // ── CreatePost ─────────────────────────────────────────────────────────────
    public async Task<PostDto> CreatePostAsync(Guid societyId, Guid userId, CreatePostRequest request)
    {
        var isMember = await db.SocietyMembers
            .AnyAsync(sm => sm.SocietyId == societyId && sm.UserId == userId);
        if (!isMember) throw new AppException("You are not a member of this society.", 403);

        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorId = userId,
            Content = request.Content,
            SocietyId = societyId,
            CreatedAt = DateTime.UtcNow
        };

        await db.Societies
            .Where(s => s.Id == societyId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.PostsCount, x => x.PostsCount + 1));

        await db.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.PostsCount, u => u.PostsCount + 1));

        db.Posts.Add(post);
        await db.SaveChangesAsync();

        // Reload with includes
        var created = await db.Posts
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.Media.OrderBy(m => m.DisplayOrder))
            .Include(p => p.PostHashTags).ThenInclude(ph => ph.HashTag)
            .FirstAsync(p => p.Id == post.Id);

        return BuildPostDto(created, userId, false, false);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private static SocietyDto ToDto(Society s, SocietyMember? membership) =>
        new(
            s.Id,
            s.Name,
            s.Description,
            s.AvatarUrl,
            s.CreatedByUserId,
            s.CreatedBy.Username,
            s.MembersCount,
            s.PostsCount,
            s.CreatedAt,
            membership != null,
            membership?.Role == SocietyRole.Admin
        );

    private static PostDto BuildPostDto(Post p, Guid uid, bool isLiked, bool isBookmarked) =>
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
            false,
            p.AuthorId == uid,
            p.Media.OrderBy(m => m.DisplayOrder)
                   .Select(m => new PostMediaDto(m.Id, m.Url, m.MediaType.ToString(), m.Width, m.Height, m.DisplayOrder))
                   .ToList(),
            p.PostHashTags.Select(ph => ph.HashTag.Name).ToList(),
            p.ParentPostId,
            p.RepostOfId,
            false
        );
}
