using Microsoft.AspNetCore.Mvc;
using NaviProject.Core.Services;

namespace NaviProject.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConversationController(ConversationService conversationService) : ControllerBase
{
    [HttpPost("{chatId}")]
    public async Task<IActionResult> Chat(int chatId, [FromBody] ChatRequest request)
    {
        var response = await conversationService.ChatAsync(chatId, request.Message);
        return Ok(new { response });
    }
}

public record ChatRequest(string Message);