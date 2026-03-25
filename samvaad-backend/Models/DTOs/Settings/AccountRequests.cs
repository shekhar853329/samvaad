using System.ComponentModel.DataAnnotations;

namespace samvaad_backend.Models.DTOs.Settings;

public class UpdateEmailRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string NewEmail { get; set; } = string.Empty;

    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
}

public class UpdatePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string NewPassword { get; set; } = string.Empty;
}

public class DeleteAccountRequest
{
    [Required]
    public string Password { get; set; } = string.Empty;
}
