using System.ComponentModel.DataAnnotations;

namespace samvaad_backend.Models.DTOs.Auth;

public class LoginRequest
{
    /// <summary>Accepts either an email address or a username.</summary>
    [Required]
    public string Identifier { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
