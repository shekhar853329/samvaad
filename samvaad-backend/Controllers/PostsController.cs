using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using samvaad_backend.Extensions;
using samvaad_backend.Models.DTOs.Posts;
using samvaad_backend.Services.Interfaces;
using System.Net.Mime;

namespace samvaad_backend.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController(IPostService postService) : ControllerBase
{
    /// <summary>Create a new post (JSON, no media).</summary>
    [Authorize]
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest request)
    {
        var userId = User.GetUserId();
        var post = await postService.CreateAsync(userId, request);
        return StatusCode(201, post);
    }

    /// <summary>Create a new post with media attachments (multipart/form-data).</summary>
    [Authorize]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateWithMedia([FromForm] CreatePostRequest request)
    {
        var userId = User.GetUserId();
        var post = await postService.CreateAsync(userId, request);
        return StatusCode(201, post);
    }

    /// <summary>Get a single post by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        Guid? requestingUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
        var post = await postService.GetByIdAsync(id, requestingUserId);
        return Ok(post);
    }

    /// <summary>Delete own post.</summary>
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();
        await postService.DeleteAsync(id, userId);
        return NoContent();
    }

    /// <summary>Like a post.</summary>
    [Authorize]
    [HttpPost("{id:guid}/like")]
    public async Task<IActionResult> Like(Guid id)
    {
        var userId = User.GetUserId();
        var post = await postService.LikeAsync(id, userId);
        return Ok(post);
    }

    /// <summary>Unlike a post.</summary>
    [Authorize]
    [HttpDelete("{id:guid}/like")]
    public async Task<IActionResult> Unlike(Guid id)
    {
        var userId = User.GetUserId();
        var post = await postService.UnlikeAsync(id, userId);
        return Ok(post);
    }

    /// <summary>Bookmark a post.</summary>
    [Authorize]
    [HttpPost("{id:guid}/bookmark")]
    public async Task<IActionResult> Bookmark(Guid id)
    {
        var userId = User.GetUserId();
        var post = await postService.BookmarkAsync(id, userId);
        return Ok(post);
    }

    /// <summary>Remove bookmark from a post.</summary>
    [Authorize]
    [HttpDelete("{id:guid}/bookmark")]
    public async Task<IActionResult> Unbookmark(Guid id)
    {
        var userId = User.GetUserId();
        var post = await postService.UnbookmarkAsync(id, userId);
        return Ok(post);
    }

    /// <summary>Repost a post.</summary>
    [Authorize]
    [HttpPost("{id:guid}/repost")]
    public async Task<IActionResult> Repost(Guid id)
    {
        var userId = User.GetUserId();
        var post = await postService.RepostAsync(id, userId);
        return Ok(post);
    }

    /// <summary>Undo a repost.</summary>
    [Authorize]
    [HttpDelete("{id:guid}/repost")]
    public async Task<IActionResult> Unrepost(Guid id)
    {
        var userId = User.GetUserId();
        var post = await postService.UnrepostAsync(id, userId);
        return Ok(post);
    }

    /// <summary>Get replies to a post (paginated).</summary>
    [HttpGet("{id:guid}/replies")]
    public async Task<IActionResult> GetReplies(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        Guid? requestingUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
        var replies = await postService.GetRepliesAsync(id, requestingUserId, page, pageSize);
        return Ok(replies);
    }

    /// <summary>Get the home feed for the logged-in user.</summary>
    [Authorize]
    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.GetUserId();
        var feed = await postService.GetFeedAsync(userId, page, pageSize);
        return Ok(feed);
    }

    /// <summary>Get all posts by a username.</summary>
    [HttpGet("by/{username}")]
    public async Task<IActionResult> GetByUsername(string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        Guid? requestingUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
        var posts = await postService.GetUserPostsAsync(username, requestingUserId, page, pageSize);
        return Ok(posts);
    }
}
