namespace samvaad_backend.Models.DTOs.Settings;

public record SessionDto(
    Guid Id,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsCurrent
);
