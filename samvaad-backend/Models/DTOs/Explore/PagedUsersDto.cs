using samvaad_backend.Models.DTOs.Users;

namespace samvaad_backend.Models.DTOs.Explore;

public record PagedUsersDto(List<FollowerDto> Users, bool HasMore);
