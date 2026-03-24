using System.ComponentModel.DataAnnotations;

namespace samvaad_backend.Models.DTOs.Auth;

public class RegisterRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(50)]
    [RegularExpression(@"^[a-z0-9._]+$", ErrorMessage = "Username may only contain lowercase letters, numbers, dots, and underscores.")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
