using NaviProject.Core.Interfaces;
using NaviProject.Core.Models;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace NaviProject.Api.Services;

public class OllamaLanguageModelService(OllamaApiClient ollamaClient) : ILanguageModelService
{
    private const string Model = "qwen3:8b";

    public async Task<string> CompleteAsync(string prompt, IEnumerable<ChatMessage> history)
    {
        var messages = new List<Message>();

        // Add System prompt
        messages.Add(new Message
        {
            Role = ChatRole.System,
            Content = """
                You are a personal assistant that helps users review their past tasks, projects, and work records.
                Your name is BX Fairy.
                Only answer based on the provided context. If the answer cannot be found in the context, say so clearly.
                Always respond in either English or Chinese, following the language of the user's question.
                If the user writes in Chinese, respond in Chinese. If the user writes in English, respond in English.
                If relevant links are available in the context, always include them in your response.
                Do not fabricate or guess information that is not in the context.
                """
        });

        // Add history context
        foreach (var msg in history)
        {
            messages.Add(new Message
            {
                Role = msg.Role == "user" ? ChatRole.User : ChatRole.Assistant,
                Content = msg.Content
            });
        }

        // Add current question（including RAG context）
        messages.Add(new Message
        {
            Role = ChatRole.User,
            Content = prompt
        });

        var response = string.Empty;
        await foreach (var chunk in ollamaClient.ChatAsync(new ChatRequest
        {
            Model = Model,
            Messages = messages,
            Stream = true
        }))
        {
            response += chunk?.Message?.Content ?? string.Empty;
        }

        return response;
    }
}