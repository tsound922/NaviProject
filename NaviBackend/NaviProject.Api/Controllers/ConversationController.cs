using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaviProject.Api.Extensions;
using NaviProject.Core.Services;

namespace NaviProject.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversationController(ConversationService conversationService) : ControllerBase
{
    [HttpPost("{chatId}")]
    public async Task<IActionResult> Chat(int chatId, [FromBody] ChatRequest request)
    {
        var userId = User.GetUserId();
        var response = await conversationService.ChatAsync(chatId, request.Message, userId);
        return Ok(new { response });
    }
}

public record ChatRequest(string Message);