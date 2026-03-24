using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using samvaad_backend.Extensions;
using samvaad_backend.Models.DTOs.Messages;
using samvaad_backend.Services.Interfaces;

namespace samvaad_backend.Controllers;

[ApiController]
[Route("api/conversations")]
[Authorize]
public class MessagesController(IMessageService messages) : ControllerBase
{
    private Guid UserId => User.GetUserId();

    [HttpGet]
    public async Task<IActionResult> GetConversations()
        => Ok(await messages.GetConversationsAsync(UserId));

    [HttpPost("direct")]
    public async Task<IActionResult> GetOrCreateDirect([FromBody] StartDirectConversationRequest req)
        => Ok(await messages.GetOrCreateDirectAsync(UserId, req.TargetUsername));

    [HttpGet("{id:guid}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
        => Ok(await messages.GetMessagesAsync(id, UserId, page, pageSize));

    [HttpPost("{id:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendMessageRequest req)
        => Ok(await messages.SendMessageAsync(id, UserId, req.Content));

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        await messages.MarkReadAsync(id, UserId);
        return NoContent();
    }
}
