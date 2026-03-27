using System.ComponentModel.DataAnnotations;

namespace samvaad_backend.Models.DTOs.Auth;

public class GoogleLoginRequest
{
    [Required]
    public string Credential { get; set; } = string.Empty;
}
