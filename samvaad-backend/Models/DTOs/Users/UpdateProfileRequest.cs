using System.ComponentModel.DataAnnotations;

namespace samvaad_backend.Models.DTOs.Users;

public class UpdateProfileRequest
{
    [MaxLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9_.]{3,50}$",
        ErrorMessage = "Username may only contain letters, digits, underscores and dots (3–50 chars).")]
    public string? Username { get; set; }

    [MaxLength(100)]
    public string? DisplayName { get; set; }

    [MaxLength(300)]
    public string? Bio { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    [MaxLength(200)]
    public string? Website { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    /// <summary>Full replacement list of interest tags (e.g. ["dotnet","angular"]). Null = no change.</summary>
    public List<string>? Tags { get; set; }
}
