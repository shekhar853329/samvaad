using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using samvaad_backend.Extensions;
using samvaad_backend.Models.DTOs.Posts;
using samvaad_backend.Models.DTOs.Societies;
using samvaad_backend.Services.Interfaces;
using System.Net.Mime;

namespace samvaad_backend.Controllers;

[ApiController]
[Route("api/societies")]
[Authorize]
public class SocietiesController(ISocietyService societyService) : ControllerBase
{
    /// <summary>Create a new society.</summary>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> Create([FromBody] CreateSocietyRequest request)
    {
        var userId = User.GetUserId();
        var society = await societyService.CreateAsync(userId, request);
        return StatusCode(201, society);
    }

    /// <summary>List all societies (paginated).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.GetUserId();
        var result = await societyService.GetAllAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get societies the current user is a member of.</summary>
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.GetUserId();
        var result = await societyService.GetUserSocietiesAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get a society by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = User.GetUserId();
        var society = await societyService.GetByIdAsync(id, userId);
        return Ok(society);
    }

    /// <summary>Join a society.</summary>
    [HttpPost("{id:guid}/join")]
    public async Task<IActionResult> Join(Guid id)
    {
        var userId = User.GetUserId();
        var society = await societyService.JoinAsync(id, userId);
        return Ok(society);
    }

    /// <summary>Leave a society.</summary>
    [HttpDelete("{id:guid}/leave")]
    public async Task<IActionResult> Leave(Guid id)
    {
        var userId = User.GetUserId();
        await societyService.LeaveAsync(id, userId);
        return NoContent();
    }

    /// <summary>Get society members (members only).</summary>
    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid id)
    {
        var userId = User.GetUserId();
        var members = await societyService.GetMembersAsync(id, userId);
        return Ok(members);
    }

    /// <summary>Get posts in a society (members only).</summary>
    [HttpGet("{id:guid}/feed")]
    public async Task<IActionResult> GetFeed(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.GetUserId();
        var feed = await societyService.GetFeedAsync(id, userId, page, pageSize);
        return Ok(feed);
    }

    /// <summary>Post to a society (members only).</summary>
    [HttpPost("{id:guid}/posts")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> CreatePost(Guid id, [FromBody] CreatePostRequest request)
    {
        var userId = User.GetUserId();
        var post = await societyService.CreatePostAsync(id, userId, request);
        return StatusCode(201, post);
    }
}
