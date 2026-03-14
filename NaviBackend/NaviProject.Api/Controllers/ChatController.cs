using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaviProject.Core.Models;
using NaviProject.Core.Services;

namespace NaviProject.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ChatController(ChatService chatService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateChat([FromBody] string? title)
    {
        var chatId = await chatService.StartNewChatAsync(title);
        return Ok(new { chatId });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllChats()
    {
        var chats = await chatService.GetAllChatsAsync();
        return Ok(chats);
    }

    [HttpGet("{chatId}/messages")]
    public async Task<IActionResult> GetMessages(int chatId)
    {
        var messages = await chatService.GetChatHistoryAsync(chatId);
        return Ok(messages);
    }

    [HttpPost("{chatId}/messages")]
    public async Task<IActionResult> SendMessage(int chatId, [FromBody] SendMessageRequest request)
    {
        await chatService.SaveMessageAsync(chatId, "user", request.Content);
        return Ok();
    }

    [HttpDelete("{chatId}")]
    public async Task<IActionResult> DeleteChat(int chatId)
    {
        await chatService.DeleteChatAsync(chatId);
        return NoContent();
    }
}

public record SendMessageRequest(string Content);