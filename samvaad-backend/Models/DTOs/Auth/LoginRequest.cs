namespace samvaad_backend.Models.DTOs.Auth;

public class LoginRequest
{
    /// <summary>Accepts either an email address or a username.</summary>
    public string Identifier { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
