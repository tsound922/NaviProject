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
        if (!chunks.Any())
            return userMessage;

        var contextBuilder = new System.Text.StringBuilder();

        foreach (var chunk in chunks)
        {
            contextBuilder.AppendLine(chunk.Content);

            // 如果有 metadata，加上 URL
            if (!string.IsNullOrEmpty(chunk.Metadata))
            {
                try
                {
                    var metadata = System.Text.Json.JsonDocument.Parse(chunk.Metadata).RootElement;
                    if (metadata.TryGetProperty("url", out var url) && url.ValueKind != System.Text.Json.JsonValueKind.Null)
                    {
                        contextBuilder.AppendLine($"Link: {url.GetString()}");
                    }
                }
                catch { }
            }

            contextBuilder.AppendLine();
        }

        return $"""
            The following are relevant records. Please use them to answer the question.
            If there are links available, include them in your response.
            Always respond in the same language as the user's question.

            {contextBuilder}

            ---
            Question: {userMessage}
            """;
    }
}