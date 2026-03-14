using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaviProject.Api.Extensions;
using NaviProject.Core.Services;

namespace NaviProject.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController(ChatService chatService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateChat([FromBody] string? title)
    {
        var userId = User.GetUserId();
        var chatId = await chatService.StartNewChatAsync(title, userId);
        return Ok(new { chatId });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllChats()
    {
        var userId = User.GetUserId();
        var chats = await chatService.GetAllChatsAsync(userId);
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
        await chatService.SaveUserMessageAsync(chatId, request.Content);
        return Ok();
    }

    [HttpDelete("{chatId}")]
    public async Task<IActionResult> DeleteChat(int chatId)
    {
        var userId = User.GetUserId();
        await chatService.DeleteChatAsync(chatId, userId);
        return NoContent();
    }
}

public record SendMessageRequest(string Content);