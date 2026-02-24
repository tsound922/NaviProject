using NaviProject.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NaviProject.Core.Services;
using NaviProject.Core.Interfaces;

namespace NaviProject.Core.Services
{
    public class ConversationService(
    ChatService chatService,
    RagService ragService,
    ILanguageModelService languageModelService)
    {
        public async Task<string> ChatAsync(int chatId, string userMessage)
        {
            // 1. Save User info
            await chatService.SaveUserMessageAsync(chatId, userMessage);

            // 2. Query relative knowledge from knowledge bank
            var relevantChunks = await ragService.SearchAsync(userMessage, topK: 3);

            // 3. Acquire history context 
            var history = await chatService.GetChatHistoryAsync(chatId);

            // 4. Build prompt
            var prompt = BuildPrompt(userMessage, relevantChunks, history);

            // 5. Call model
            var response = await languageModelService.CompleteAsync(prompt, history);

            // 6. Save the response from model
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
}
