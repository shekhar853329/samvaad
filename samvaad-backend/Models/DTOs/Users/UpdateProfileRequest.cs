namespace samvaad_backend.Models.DTOs.Users;

public class UpdateProfileRequest
{
    public string? Username { get; set; }

    public string? DisplayName { get; set; }

    public string? Bio { get; set; }

    public string? Location { get; set; }

    public string? Website { get; set; }

    public string? AvatarUrl { get; set; }

    public string? CoverImageUrl { get; set; }

    /// <summary>Full replacement list of interest tags (e.g. ["dotnet","angular"]). Null = no change.</summary>
    public List<string>? Tags { get; set; }
}
