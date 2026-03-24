using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using samvaad_backend.Models.DTOs.Auth;
using samvaad_backend.Services.Interfaces;
using System.Net.Mime;

namespace samvaad_backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);
        return StatusCode(201, result);
    }

    [HttpPost("login")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await authService.RefreshAsync(request.RefreshToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        await authService.LogoutAsync(request.RefreshToken);
        return NoContent();
    }

    /// <summary>Returns the current user's identity. Requires a valid access token.</summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;
        var username = User.FindFirst("username")?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                 ?? User.FindFirst("email")?.Value;

        return Ok(new { userId, username, email });
    }
}
