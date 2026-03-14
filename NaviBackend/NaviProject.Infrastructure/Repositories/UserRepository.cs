using NaviProject.Core.Interfaces;
using NaviProject.Core.Models;
using Npgsql;
using Dapper;

namespace NaviProject.Infrastructure.Repositories
{
    public class UserRepository(string connectionString) : IUserRepository
    {
        private NpgsqlConnection CreateConnection() => new(connectionString);

        public async Task<AppUser?> GetByUsernameAsync(string username)
        {
            using var conn = CreateConnection();
            const string sql = """
            SELECT 
                id,
                username,
                email,
                password_hash AS PasswordHash,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM app_user 
            WHERE username = @Username;
            """;
            return await conn.QueryFirstOrDefaultAsync<AppUser>(sql, new { Username = username });
        }

        public async Task<AppUser?> GetByEmailAsync(string email)
        {
            using var conn = CreateConnection();
            const string sql = """
            SELECT 
                id,
                username,
                email,
                password_hash AS PasswordHash,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM app_user 
            WHERE email = @Email;
            """;
            return await conn.QueryFirstOrDefaultAsync<AppUser>(sql, new { Email = email });
        }

        public async Task<int> CreateUserAsync(string username, string email, string passwordHash)
        {
            using var conn = CreateConnection();
            const string sql = """
            INSERT INTO app_user (username, email, password_hash)
            VALUES (@Username, @Email, @PasswordHash)
            RETURNING id;
            """;
            return await conn.ExecuteScalarAsync<int>(sql, new { Username = username, Email = email, PasswordHash = passwordHash });
        }
    }
}
