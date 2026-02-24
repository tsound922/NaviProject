using Dapper;
using NaviProject.Core.Interfaces;
using NaviProject.Core.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviProject.Infrastructure.Repositories
{
    public class ChatMessageRepository(string connectionString) : IChatMessageRepository
    {
        private NpgsqlConnection CreateConnection() => new(connectionString);

        public async Task AddMessageAsync(int chatId, string role, string content)
        {
            using var conn = CreateConnection();
            const string sql = """
            INSERT INTO chat_message (chat_id, role, content) VALUES (@ChatId, @Role, @Content);
            """;
            await conn.ExecuteAsync(sql, new { ChatId = chatId, Role = role, Content = content });
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesByChatIdAsync(int chatId)
        {
            using var conn = CreateConnection();
            const string sql = """
                        SELECT 
                            id,
                            chat_id AS ChatId,
                            role,
                            content,
                            created_at AS CreatedAt
                        FROM chat_message 
                        WHERE chat_id = @ChatId 
                        ORDER BY created_at ASC;
                        """;
            return await conn.QueryAsync<ChatMessage>(sql, new { ChatId = chatId });
        }
    }
}
