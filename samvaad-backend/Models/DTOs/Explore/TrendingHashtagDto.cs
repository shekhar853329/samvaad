namespace samvaad_backend.Models.DTOs.Explore;

public record TrendingHashtagDto(Guid Id, string Name, string? Category, int PostCount);
