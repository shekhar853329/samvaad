using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using samvaad_backend.Extensions;
using samvaad_backend.Models.DTOs.Settings;
using samvaad_backend.Services.Interfaces;
using System.Net.Mime;

namespace samvaad_backend.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsController(ISettingsService settingsService) : ControllerBase
{
    // ── Notification preferences ──────────────────────────────────────────────

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotificationPrefs()
    {
        var userId = User.GetUserId();
        return Ok(await settingsService.GetNotificationPrefsAsync(userId));
    }

    [HttpPut("notifications")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> SaveNotificationPrefs([FromBody] SaveNotificationPrefsRequest request)
    {
        var userId = User.GetUserId();
        return Ok(await settingsService.SaveNotificationPrefsAsync(userId, request));
    }

    // ── Privacy settings ──────────────────────────────────────────────────────

    [HttpGet("privacy")]
    public async Task<IActionResult> GetPrivacySettings()
    {
        var userId = User.GetUserId();
        return Ok(await settingsService.GetPrivacySettingsAsync(userId));
    }

    [HttpPut("privacy")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> SavePrivacySettings([FromBody] SavePrivacySettingsRequest request)
    {
        var userId = User.GetUserId();
        return Ok(await settingsService.SavePrivacySettingsAsync(userId, request));
    }

    // ── Feed preferences ──────────────────────────────────────────────────────

    [HttpGet("feed")]
    public async Task<IActionResult> GetFeedPrefs()
    {
        var userId = User.GetUserId();
        return Ok(await settingsService.GetFeedPrefsAsync(userId));
    }

    [HttpPut("feed")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> SaveFeedPrefs([FromBody] SaveFeedPrefsRequest request)
    {
        var userId = User.GetUserId();
        return Ok(await settingsService.SaveFeedPrefsAsync(userId, request));
    }

    [HttpPost("feed/muted-words")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> AddMutedWord([FromBody] AddMutedWordRequest request)
    {
        var userId = User.GetUserId();
        return Ok(await settingsService.AddMutedWordAsync(userId, request.Word));
    }

    [HttpDelete("feed/muted-words/{word}")]
    public async Task<IActionResult> RemoveMutedWord(string word)
    {
        var userId = User.GetUserId();
        return Ok(await settingsService.RemoveMutedWordAsync(userId, word));
    }

    // ── Sessions ──────────────────────────────────────────────────────────────

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        var userId = User.GetUserId();
        return Ok(await settingsService.GetSessionsAsync(userId));
    }

    [HttpDelete("sessions/{id:guid}")]
    public async Task<IActionResult> RevokeSession(Guid id)
    {
        var userId = User.GetUserId();
        await settingsService.RevokeSessionAsync(userId, id);
        return NoContent();
    }

    [HttpDelete("sessions")]
    public async Task<IActionResult> RevokeAllOtherSessions()
    {
        var userId = User.GetUserId();
        await settingsService.RevokeAllOtherSessionsAsync(userId);
        return NoContent();
    }

    // ── Account actions ───────────────────────────────────────────────────────

    [HttpPut("account/email")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailRequest request)
    {
        var userId = User.GetUserId();
        await settingsService.UpdateEmailAsync(userId, request);
        return NoContent();
    }

    [HttpPut("account/password")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        var userId = User.GetUserId();
        await settingsService.UpdatePasswordAsync(userId, request);
        return NoContent();
    }

    [HttpDelete("account")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        var userId = User.GetUserId();
        await settingsService.DeleteAccountAsync(userId, request.Password);
        return NoContent();
    }
}
