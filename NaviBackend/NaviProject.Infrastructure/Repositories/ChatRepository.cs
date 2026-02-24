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
    public class ChatRepository(string connectionString) : IChatRepository
    {
        private NpgsqlConnection CreateConnection() => new(connectionString);

        public async Task<int> CreateChatAsync(string? title)
        {
            using var conn = CreateConnection();
            const string sql = """
            INSERT INTO chat (title) VALUES (@Title) RETURNING id;
            """;
            return await conn.ExecuteScalarAsync<int>(sql, new { Title = title });
        }

        public async Task<Chat?> GetChatByIdAsync(int chatId)
        {
            using var conn = CreateConnection();
            const string sql = """
                    SELECT 
                        id,
                        title,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM chat 
                    WHERE id = @Id;
                    """;
            return await conn.QueryFirstOrDefaultAsync<Chat>(sql, new { Id = chatId });
        }

        public async Task<IEnumerable<Chat>> GetAllChatsAsync()
        {
            using var conn = CreateConnection();
            const string sql = """
                    SELECT 
                        id,
                        title,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt
                    FROM chat 
                    ORDER BY created_at DESC;
                    """;
            return await conn.QueryAsync<Chat>(sql);
        }

        public async Task UpdateChatTitleAsync(int chatId, string title)
        {
            using var conn = CreateConnection();
            const string sql = """
            UPDATE chat SET title = @Title, updated_at = NOW() WHERE id = @Id;
            """;
            await conn.ExecuteAsync(sql, new { Title = title, Id = chatId });
        }

        public async Task DeleteChatAsync(int chatId)
        {
            using var conn = CreateConnection();
            const string sql = "DELETE FROM chat WHERE id = @Id;";
            await conn.ExecuteAsync(sql, new { Id = chatId });
        }
    }
}
