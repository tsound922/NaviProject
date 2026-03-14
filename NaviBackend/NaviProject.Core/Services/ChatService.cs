using NaviProject.Core.Interfaces;
using NaviProject.Core.Models;

namespace NaviProject.Core.Services;

public class ChatService(IChatRepository chatRepo, IChatMessageRepository messageRepo)
{
    public async Task<int> StartNewChatAsync(string? title, int userId)
    {
        return await chatRepo.CreateChatAsync(title, userId);
    }

    public async Task<IEnumerable<Chat>> GetAllChatsAsync(int userId)
    {
        return await chatRepo.GetAllChatsAsync(userId);
    }

    public async Task<IEnumerable<ChatMessage>> GetChatHistoryAsync(int chatId)
    {
        return await messageRepo.GetMessagesByChatIdAsync(chatId);
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

    public async Task DeleteChatAsync(int chatId, int userId)
    {
        await chatRepo.DeleteChatAsync(chatId, userId);
    }
}