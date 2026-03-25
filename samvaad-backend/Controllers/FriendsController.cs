using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using samvaad_backend.Extensions;
using samvaad_backend.Services.Interfaces;

namespace samvaad_backend.Controllers;

[ApiController]
[Route("api/friends")]
[Authorize]
public class FriendsController(IFriendService friendService) : ControllerBase
{
    /// <summary>Get all pending friend requests sent to the current user.</summary>
    [HttpGet("requests")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var userId = User.GetUserId();
        var requests = await friendService.GetPendingRequestsAsync(userId);
        return Ok(requests);
    }

    /// <summary>Get the current user's friends list (paginated).</summary>
    [HttpGet]
    public async Task<IActionResult> GetFriends([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.GetUserId();
        var friends = await friendService.GetFriendsAsync(userId, page, pageSize);
        return Ok(friends);
    }

    /// <summary>Send a friend request to the given username.</summary>
    [HttpPost("requests/{username}")]
    public async Task<IActionResult> SendRequest(string username)
    {
        var userId = User.GetUserId();
        await friendService.SendRequestAsync(userId, username);
        return Ok();
    }

    /// <summary>Cancel a pending friend request that the current user sent.</summary>
    [HttpDelete("requests/{username}")]
    public async Task<IActionResult> CancelRequest(string username)
    {
        var userId = User.GetUserId();
        await friendService.CancelRequestAsync(userId, username);
        return NoContent();
    }

    /// <summary>Accept a friend request from the given username.</summary>
    [HttpPost("requests/{username}/accept")]
    public async Task<IActionResult> AcceptRequest(string username)
    {
        var userId = User.GetUserId();
        await friendService.AcceptRequestAsync(userId, username);
        return Ok();
    }

    /// <summary>Decline a friend request from the given username.</summary>
    [HttpPost("requests/{username}/decline")]
    public async Task<IActionResult> DeclineRequest(string username)
    {
        var userId = User.GetUserId();
        await friendService.DeclineRequestAsync(userId, username);
        return Ok();
    }

    /// <summary>Unfriend someone.</summary>
    [HttpDelete("{username}")]
    public async Task<IActionResult> Unfriend(string username)
    {
        var userId = User.GetUserId();
        await friendService.UnfriendAsync(userId, username);
        return NoContent();
    }
}
