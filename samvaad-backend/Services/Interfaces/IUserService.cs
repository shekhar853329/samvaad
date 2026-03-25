using samvaad_backend.Models.DTOs.Users;

namespace samvaad_backend.Services.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(string username, Guid? requestingUserId);
    Task<UserProfileDto> GetMyProfileAsync(Guid userId);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task FollowAsync(Guid followerId, string targetUsername);
    Task UnfollowAsync(Guid followerId, string targetUsername);
    Task<IReadOnlyList<FollowerDto>> GetFollowersAsync(string username, Guid? requestingUserId, int page, int pageSize);
    Task<IReadOnlyList<FollowerDto>> GetFollowingAsync(string username, Guid? requestingUserId, int page, int pageSize);
    Task<IReadOnlyList<FollowerDto>> GetWhoToFollowAsync(Guid userId, int count);
}
