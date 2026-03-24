namespace samvaad_backend.Models.DTOs.Posts;

public record PostMediaDto(
    Guid Id,
    string Url,
    string MediaType,
    int? Width,
    int? Height,
    int DisplayOrder
);
