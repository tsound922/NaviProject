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
            Content = "你是一个个人助手，帮助用户回顾和总结他们过去做过的任务和项目。请根据提供的历史记录内容回答问题。"
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