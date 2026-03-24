using Microsoft.AspNetCore.Mvc;
using samvaad_backend.Extensions;
using samvaad_backend.Services.Interfaces;

namespace samvaad_backend.Controllers;

[ApiController]
[Route("api/explore")]
public class ExploreController(IExploreService exploreService) : ControllerBase
{
    /// <summary>Top trending hashtags by post count.</summary>
    [HttpGet("trending/hashtags")]
    public async Task<IActionResult> GetTrendingHashtags([FromQuery] int count = 10)
    {
        count = Math.Clamp(count, 1, 50);
        var result = await exploreService.GetTrendingHashtagsAsync(count);
        return Ok(result);
    }

    /// <summary>Most-engaged posts from the last 7 days.</summary>
    [HttpGet("trending/posts")]
    public async Task<IActionResult> GetTrendingPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : (Guid?)null;
        var result = await exploreService.GetTrendingPostsAsync(userId, page, Math.Min(pageSize, 50));
        return Ok(result);
    }

    /// <summary>Posts containing a specific hashtag.</summary>
    [HttpGet("hashtag/{tag}")]
    public async Task<IActionResult> GetByHashtag(string tag, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : (Guid?)null;
        var result = await exploreService.GetPostsByHashtagAsync(tag, userId, page, Math.Min(pageSize, 50));
        return Ok(result);
    }

    /// <summary>Search posts by keyword.</summary>
    [HttpGet("search/posts")]
    public async Task<IActionResult> SearchPosts([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { error = "Search query cannot be empty." });

        var userId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : (Guid?)null;
        var result = await exploreService.SearchPostsAsync(q.Trim(), userId, page, Math.Min(pageSize, 50));
        return Ok(result);
    }

    /// <summary>Search users by username or display name.</summary>
    [HttpGet("search/users")]
    public async Task<IActionResult> SearchUsers([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { error = "Search query cannot be empty." });

        var userId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : (Guid?)null;
        var result = await exploreService.SearchUsersAsync(q.Trim(), userId, page, Math.Min(pageSize, 50));
        return Ok(result);
    }
}
