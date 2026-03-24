using System.ComponentModel.DataAnnotations;

namespace samvaad_backend.Models.DTOs.Auth;

public class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
