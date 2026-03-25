using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using samvaad_backend.Common;
using samvaad_backend.Extensions;
using samvaad_backend.Models.DTOs.Users;
using samvaad_backend.Services.Interfaces;
using System.Net.Mime;

namespace samvaad_backend.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService, IWebHostEnvironment env) : ControllerBase
{
    private static readonly HashSet<string> _allowedImageExts =
        [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    /// <summary>Get the currently authenticated user's own profile (with tags).</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.GetUserId();
        var profile = await userService.GetMyProfileAsync(userId);
        return Ok(profile);
    }

    /// <summary>Get a user's public profile by username.</summary>
    [HttpGet("{username}")]
    public async Task<IActionResult> GetProfile(string username)
    {
        Guid? requestingUserId = User.Identity?.IsAuthenticated == true
            ? User.GetUserId()
            : null;

        var profile = await userService.GetProfileAsync(username, requestingUserId);
        return Ok(profile);
    }

    /// <summary>Update the current user's profile.</summary>
    [Authorize]
    [HttpPut("me")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.GetUserId();
        var updated = await userService.UpdateProfileAsync(userId, request);
        return Ok(updated);
    }

    /// <summary>Upload a new avatar image. Returns the served URL.</summary>
    [Authorize]
    [HttpPost("me/avatar")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new AppException("No file provided.", 400);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedImageExts.Contains(ext))
            throw new AppException("Only JPG, PNG, GIF and WebP files are allowed.", 400);

        if (file.Length > 5 * 1024 * 1024)
            throw new AppException("File size must be under 5 MB.", 400);

        var userId = User.GetUserId();
        var uploadsDir = Path.Combine(env.WebRootPath ?? "wwwroot", "avatars");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{userId}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream);

        var avatarUrl = $"/avatars/{fileName}";
        await userService.UpdateProfileAsync(userId, new UpdateProfileRequest { AvatarUrl = avatarUrl });

        return Ok(new { avatarUrl });
    }

    /// <summary>Follow a user.</summary>
    [Authorize]
    [HttpPost("{username}/follow")]
    public async Task<IActionResult> Follow(string username)
    {
        var followerId = User.GetUserId();
        await userService.FollowAsync(followerId, username);
        return NoContent();
    }

    /// <summary>Unfollow a user.</summary>
    [Authorize]
    [HttpDelete("{username}/follow")]
    public async Task<IActionResult> Unfollow(string username)
    {
        var followerId = User.GetUserId();
        await userService.UnfollowAsync(followerId, username);
        return NoContent();
    }

    /// <summary>Get followers of a user (paginated).</summary>
    [HttpGet("{username}/followers")]
    public async Task<IActionResult> GetFollowers(
        string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        Guid? requestingUserId = User.Identity?.IsAuthenticated == true
            ? User.GetUserId()
            : null;

        var followers = await userService.GetFollowersAsync(username, requestingUserId, page, pageSize);
        return Ok(followers);
    }

    /// <summary>Get users that a user is following (paginated).</summary>
    [HttpGet("{username}/following")]
    public async Task<IActionResult> GetFollowing(
        string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        Guid? requestingUserId = User.Identity?.IsAuthenticated == true
            ? User.GetUserId()
            : null;

        var following = await userService.GetFollowingAsync(username, requestingUserId, page, pageSize);
        return Ok(following);
    }

    /// <summary>Get suggested users to follow for the logged-in user.</summary>
    [Authorize]
    [HttpGet("who-to-follow")]
    public async Task<IActionResult> WhoToFollow([FromQuery] int count = 5)
    {
        var userId = User.GetUserId();
        var suggestions = await userService.GetWhoToFollowAsync(userId, count);
        return Ok(suggestions);
    }
}
