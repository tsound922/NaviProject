using NaviProject.Core.Interfaces;
using NaviProject.Core.Models;

namespace NaviProject.Core.Services;

public class ChatService(IChatRepository chatRepo, IChatMessageRepository messageRepo)
{
    public async Task<int> StartNewChatAsync(string? title = null)
    {
        return await chatRepo.CreateChatAsync(title);
    }

    public async Task<IEnumerable<Chat>> GetAllChatsAsync()
    {
        return await chatRepo.GetAllChatsAsync();
    }

    public async Task<IEnumerable<ChatMessage>> GetChatHistoryAsync(int chatId)
    {
        return await messageRepo.GetMessagesByChatIdAsync(chatId);
    }

    public async Task SaveMessageAsync(int chatId, string role, string content)
    {
        await messageRepo.AddMessageAsync(chatId, role, content);
        await chatRepo.UpdateChatTitleAsync(chatId, content[..Math.Min(50, content.Length)]);
    }

    public async Task DeleteChatAsync(int chatId)
    {
        await chatRepo.DeleteChatAsync(chatId);
    }
    public async Task SaveUserMessageAsync(int chatId, string content)
    {
        await messageRepo.AddMessageAsync(chatId, "user", content);
        await chatRepo.UpdateChatTitleAsync(chatId, content[..Math.Min(50, content.Length)]);
    }
    public async Task SaveAssistantMessageAsync(int chatId, string content)
    {
        await messageRepo.AddMessageAsync(chatId, "assistant", content);
    }

}