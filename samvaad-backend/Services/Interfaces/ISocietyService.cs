using samvaad_backend.Models.DTOs.Posts;
using samvaad_backend.Models.DTOs.Societies;

namespace samvaad_backend.Services.Interfaces;

public interface ISocietyService
{
    Task<SocietyDto> CreateAsync(Guid userId, CreateSocietyRequest request);
    Task<SocietyDto> GetByIdAsync(Guid societyId, Guid requestingUserId);
    Task<PagedSocietiesDto> GetAllAsync(Guid requestingUserId, int page, int pageSize);
    Task<PagedSocietiesDto> GetUserSocietiesAsync(Guid userId, int page, int pageSize);
    Task<SocietyDto> JoinAsync(Guid societyId, Guid userId);
    Task LeaveAsync(Guid societyId, Guid userId);
    Task<IReadOnlyList<SocietyMemberDto>> GetMembersAsync(Guid societyId, Guid requestingUserId);
    Task<PagedPostsDto> GetFeedAsync(Guid societyId, Guid requestingUserId, int page, int pageSize);
    Task<PostDto> CreatePostAsync(Guid societyId, Guid userId, CreatePostRequest request);
}
