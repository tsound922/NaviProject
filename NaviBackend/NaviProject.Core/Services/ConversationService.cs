using NaviProject.Core.Interfaces;
using NaviProject.Core.Models;
using NaviProject.Core.Services;

namespace NaviProject.Core.Services;

public class ConversationService(
    ChatService chatService,
    RagService ragService,
    ILanguageModelService languageModelService)
{
    public async Task<string> ChatAsync(int chatId, string userMessage, int userId)
    {
        // 1. Save user information
        await chatService.SaveUserMessageAsync(chatId, userMessage);

        // 2. User based query from knowledge bank (User ID)
        var relevantChunks = await ragService.SearchAsync(userMessage, userId, topK: 3);

        // 3. Get Chat history
        var history = await chatService.GetChatHistoryAsync(chatId);

        // 4. build prompt
        var prompt = BuildPrompt(userMessage, relevantChunks, history);

        // 5. Call language model API
        var response = await languageModelService.CompleteAsync(prompt, history);

        // 6. Save response from model
        await chatService.SaveAssistantMessageAsync(chatId, response);

        return response;
    }

    private static string BuildPrompt(
        string userMessage,
        IEnumerable<RagChunk> chunks,
        IEnumerable<ChatMessage> history)
    {
        var context = string.Join("\n\n", chunks.Select(c => c.Content));

        return string.IsNullOrEmpty(context)
            ? userMessage
            : $"""
               以下是相关的历史任务记录，请参考这些内容回答问题：

               {context}

               ---
               问题：{userMessage}
               """;
    }
}