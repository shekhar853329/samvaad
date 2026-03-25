using Microsoft.EntityFrameworkCore;
using samvaad_backend.Common;
using samvaad_backend.Data;
using samvaad_backend.Models.DTOs.Users;
using samvaad_backend.Models.Entities;
using samvaad_backend.Models.Enums;
using samvaad_backend.Services.Interfaces;

namespace samvaad_backend.Services;

public class UserService(AppDbContext db, INotificationService notifications) : IUserService
{
    public async Task<UserProfileDto> GetProfileAsync(string username, Guid? requestingUserId)
    {
        var user = await db.Users
            .AsNoTracking()
            .Include(u => u.Tags)
            .FirstOrDefaultAsync(u => u.Username == username.ToLowerInvariant())
            ?? throw new AppException($"User '{username}' not found.", 404);

        bool isFollowing = false;
        bool isOwnProfile = requestingUserId.HasValue && requestingUserId.Value == user.Id;

        if (requestingUserId.HasValue && !isOwnProfile)
        {
            isFollowing = await db.Follow.AnyAsync(f =>
                f.FollowerId == requestingUserId.Value && f.FollowingId == user.Id);
        }

        return MapToProfileDto(user, isFollowing, isOwnProfile);
    }

    public async Task<UserProfileDto> GetMyProfileAsync(Guid userId)
    {
        var user = await db.Users
            .AsNoTracking()
            .Include(u => u.Tags)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new AppException("User not found.", 404);

        return MapToProfileDto(user, isFollowing: false, isOwnProfile: true);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await db.Users
            .Include(u => u.Tags)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new AppException("User not found.", 404);

        if (request.Username is not null)
        {
            var normalized = request.Username.ToLowerInvariant();
            if (normalized != user.Username)
            {
                var taken = await db.Users.AnyAsync(u => u.Username == normalized && u.Id != userId);
                if (taken)
                    throw new AppException("Username is already taken.", 409);
                user.Username = normalized;
            }
        }

        if (request.DisplayName is not null) user.DisplayName = request.DisplayName;
        if (request.Bio is not null) user.Bio = request.Bio;
        if (request.Location is not null) user.Location = request.Location;
        if (request.Website is not null) user.Website = request.Website;
        if (request.AvatarUrl is not null) user.AvatarUrl = request.AvatarUrl;
        if (request.CoverImageUrl is not null) user.CoverImageUrl = request.CoverImageUrl;

        if (request.Tags is not null)
        {
            // Replace all tags with the new set (max 10, each max 50 chars)
            db.UserTags.RemoveRange(user.Tags);
            var newTags = request.Tags
                .Select(t => t.Trim().ToLowerInvariant().TrimStart('#'))
                .Where(t => t.Length > 0 && t.Length <= 50)
                .Distinct()
                .Take(10)
                .Select(t => new UserTag { UserId = userId, Name = t })
                .ToList();
            db.UserTags.AddRange(newTags);
        }

        await db.SaveChangesAsync();

        // Reload tags after save
        await db.Entry(user).Collection(u => u.Tags).LoadAsync();

        return MapToProfileDto(user, isFollowing: false, isOwnProfile: true);
    }

    public async Task FollowAsync(Guid followerId, string targetUsername)
    {
        var target = await db.Users
            .FirstOrDefaultAsync(u => u.Username == targetUsername.ToLowerInvariant())
            ?? throw new AppException($"User '{targetUsername}' not found.", 404);

        if (target.Id == followerId)
            throw new AppException("You cannot follow yourself.", 400);

        var alreadyFollowing = await db.Follow.AnyAsync(f =>
            f.FollowerId == followerId && f.FollowingId == target.Id);

        if (alreadyFollowing)
            throw new AppException("You are already following this user.", 409);

        db.Follow.Add(new Follow
        {
            FollowerId = followerId,
            FollowingId = target.Id,
            CreatedAt = DateTime.UtcNow
        });

        // Increment denormalised counters atomically
        await db.Users
            .Where(u => u.Id == followerId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.FollowingCount, u => u.FollowingCount + 1));

        await db.Users
            .Where(u => u.Id == target.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.FollowersCount, u => u.FollowersCount + 1));

        await db.SaveChangesAsync();

        // Notify followed user
        await notifications.CreateAsync(target.Id, followerId, NotificationType.Follow);
    }

    public async Task UnfollowAsync(Guid followerId, string targetUsername)
    {
        var target = await db.Users
            .FirstOrDefaultAsync(u => u.Username == targetUsername.ToLowerInvariant())
            ?? throw new AppException($"User '{targetUsername}' not found.", 404);

        var follow = await db.Follow
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == target.Id)
            ?? throw new AppException("You are not following this user.", 404);

        db.Follow.Remove(follow);

        await db.Users
            .Where(u => u.Id == followerId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.FollowingCount, u => u.FollowingCount - 1));

        await db.Users
            .Where(u => u.Id == target.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.FollowersCount, u => u.FollowersCount - 1));

        await db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<FollowerDto>> GetFollowersAsync(
        string username, Guid? requestingUserId, int page, int pageSize)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username.ToLowerInvariant())
            ?? throw new AppException($"User '{username}' not found.", 404);

        var followers = await db.Follow
            .AsNoTracking()
            .Where(f => f.FollowingId == user.Id)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => f.Follower)
            .ToListAsync();

        return await MapToFollowerDtos(followers, requestingUserId);
    }

    public async Task<IReadOnlyList<FollowerDto>> GetFollowingAsync(
        string username, Guid? requestingUserId, int page, int pageSize)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username.ToLowerInvariant())
            ?? throw new AppException($"User '{username}' not found.", 404);

        var following = await db.Follow
            .AsNoTracking()
            .Where(f => f.FollowerId == user.Id)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => f.Following)
            .ToListAsync();

        return await MapToFollowerDtos(following, requestingUserId);
    }

    public async Task<IReadOnlyList<FollowerDto>> GetWhoToFollowAsync(Guid userId, int count)
    {
        // Users the current user is already following
        var alreadyFollowingIds = await db.Follow
            .AsNoTracking()
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        var suggestions = await db.Users
            .AsNoTracking()
            .Where(u => u.Id != userId && !alreadyFollowingIds.Contains(u.Id))
            .OrderByDescending(u => u.FollowersCount)
            .Take(count)
            .ToListAsync();

        return suggestions.Select(u => new FollowerDto(
            u.Id, u.Username, u.DisplayName, u.AvatarUrl, u.IsVerified, IsFollowing: false
        )).ToList();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static UserProfileDto MapToProfileDto(User u, bool isFollowing, bool isOwnProfile) =>
        new(u.Id, u.Username, u.DisplayName, u.AvatarUrl, u.CoverImageUrl,
            u.Bio, u.Location, u.Website, u.IsVerified, u.IsPrivate,
            u.PostsCount, u.FollowersCount, u.FollowingCount, u.TotalViewsCount,
            u.JoinedAt, isFollowing, isOwnProfile,
            (u.Tags ?? (ICollection<UserTag>)[]).Select(t => t.Name).ToList());

    private async Task<IReadOnlyList<FollowerDto>> MapToFollowerDtos(
        IEnumerable<User> users, Guid? requestingUserId)
    {
        if (!requestingUserId.HasValue)
            return users.Select(u => new FollowerDto(
                u.Id, u.Username, u.DisplayName, u.AvatarUrl, u.IsVerified, false)).ToList();

        var userIds = users.Select(u => u.Id).ToList();
        var followedIds = await db.Follow
            .AsNoTracking()
            .Where(f => f.FollowerId == requestingUserId.Value && userIds.Contains(f.FollowingId))
            .Select(f => f.FollowingId)
            .ToHashSetAsync();

        return users.Select(u => new FollowerDto(
            u.Id, u.Username, u.DisplayName, u.AvatarUrl, u.IsVerified,
            followedIds.Contains(u.Id))).ToList();
    }
}
