using Dapper;
using NaviProject.Core.Interfaces;
using NaviProject.Core.Models;
using Npgsql;

namespace NaviProject.Infrastructure.Repositories;

public class ChatRepository(string connectionString) : IChatRepository
{
    private NpgsqlConnection CreateConnection() => new(connectionString);

    public async Task<int> CreateChatAsync(string? title, int userId)
    {
        using var conn = CreateConnection();
        const string sql = """
            INSERT INTO chat (title, user_id) VALUES (@Title, @UserId) RETURNING id;
            """;
        return await conn.ExecuteScalarAsync<int>(sql, new { Title = title, UserId = userId });
    }

    public async Task<Chat?> GetChatByIdAsync(int chatId, int userId)
    {
        using var conn = CreateConnection();
        const string sql = """
            SELECT 
                id,
                title,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM chat 
            WHERE id = @Id AND user_id = @UserId;
            """;
        return await conn.QueryFirstOrDefaultAsync<Chat>(sql, new { Id = chatId, UserId = userId });
    }

    public async Task<IEnumerable<Chat>> GetAllChatsAsync(int userId)
    {
        using var conn = CreateConnection();
        const string sql = """
            SELECT 
                id,
                title,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM chat 
            WHERE user_id = @UserId
            ORDER BY created_at DESC;
            """;
        return await conn.QueryAsync<Chat>(sql, new { UserId = userId });
    }

    public async Task UpdateChatTitleAsync(int chatId, string title)
    {
        using var conn = CreateConnection();
        const string sql = """
            UPDATE chat SET title = @Title, updated_at = NOW() WHERE id = @Id;
            """;
        await conn.ExecuteAsync(sql, new { Title = title, Id = chatId });
    }

    public async Task DeleteChatAsync(int chatId, int userId)
    {
        using var conn = CreateConnection();
        const string sql = "DELETE FROM chat WHERE id = @Id AND user_id = @UserId;";
        await conn.ExecuteAsync(sql, new { Id = chatId, UserId = userId });
    }
}