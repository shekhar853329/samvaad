using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using samvaad_backend.Extensions;
using samvaad_backend.Services.Interfaces;

namespace samvaad_backend.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    /// <summary>Get paginated notifications for the logged-in user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = User.GetUserId();
        var result = await notificationService.GetNotificationsAsync(userId, type, page, Math.Min(pageSize, 50));
        return Ok(result);
    }

    /// <summary>Get unread notification count.</summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.GetUserId();
        var count = await notificationService.GetUnreadCountAsync(userId);
        return Ok(new { count });
    }

    /// <summary>Mark a specific notification as read.</summary>
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = User.GetUserId();
        await notificationService.MarkAsReadAsync(id, userId);
        return NoContent();
    }

    /// <summary>Mark all notifications as read.</summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.GetUserId();
        await notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }
}
