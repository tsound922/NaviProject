using Dapper;
using NaviProject.Core.Interfaces;
using NaviProject.Core.Models;
using Npgsql;
using Pgvector;

namespace NaviProject.Infrastructure.Repositories;

public class RagRepository(NpgsqlDataSource dataSource) : IRagRepository
{
    private NpgsqlConnection CreateConnection()
    {
        return dataSource.OpenConnection();
    }

    public async Task InsertChunkAsync(RagChunk chunk, int userId, bool isPublic = false)
    {
        using var conn = CreateConnection();
        const string sql = """
        INSERT INTO simp_rag (source, chunk_id, start_index, end_index, content, embedding, user_id, is_public, metadata)
        VALUES (@Source, @ChunkId, @StartIndex, @EndIndex, @Content, @Embedding, @UserId, @IsPublic, @Metadata::jsonb)
        ON CONFLICT (source, chunk_id) DO UPDATE
        SET content = EXCLUDED.content,
            embedding = EXCLUDED.embedding,
            is_public = EXCLUDED.is_public,
            metadata = EXCLUDED.metadata,
            updated_at = NOW();
        """;
        await conn.ExecuteAsync(sql, new
        {
            chunk.Source,
            chunk.ChunkId,
            chunk.StartIndex,
            chunk.EndIndex,
            chunk.Content,
            Embedding = chunk.Embedding != null ? new Vector(chunk.Embedding) : null,
            UserId = userId,
            IsPublic = isPublic,
            Metadata = chunk.Metadata
        });
    }
    public async Task<IEnumerable<RagChunk>> SearchAsync(float[] queryEmbedding, int userId, int topK = 5)
    {
        using var conn = CreateConnection();
        const string sql = """
        SELECT 
            id,
            source,
            chunk_id AS ChunkId,
            start_index AS StartIndex,
            end_index AS EndIndex,
            content,
            is_public AS IsPublic,
            metadata::text AS Metadata,
            created_at AS CreatedAt,
            updated_at AS UpdatedAt
        FROM simp_rag
        WHERE (user_id = @UserId OR is_public = TRUE)
        ORDER BY embedding <=> @Embedding
        LIMIT @TopK;
        """;
        return await conn.QueryAsync<RagChunk>(sql, new
        {
            Embedding = new Vector(queryEmbedding),
            UserId = userId,
            TopK = topK
        });
    }
    public async Task DeleteBySourceAsync(string source, int userId)
    {
        using var conn = CreateConnection();
        const string sql = "DELETE FROM simp_rag WHERE source = @Source AND user_id = @UserId;";
        await conn.ExecuteAsync(sql, new { Source = source, UserId = userId });
    }

    public async Task<IEnumerable<RagChunk>> HybridSearchAsync(float[] queryEmbedding, string query, int userId, int topK = 5)
    {
        using var conn = CreateConnection();
        const string sql = """
        SELECT 
            id,
            source,
            chunk_id AS ChunkId,
            start_index AS StartIndex,
            end_index AS EndIndex,
            content,
            is_public AS IsPublic,
            metadata::text AS Metadata,
            created_at AS CreatedAt,
            updated_at AS UpdatedAt,
            (1 - (embedding <=> @Embedding)) * 0.7 + 
            COALESCE(ts_rank(content_tsv, plainto_tsquery('english', @Query)), 0) * 0.3 AS score
        FROM simp_rag
        WHERE (user_id = @UserId OR is_public = TRUE)
        ORDER BY score DESC
        LIMIT @TopK;
        """;
        return await conn.QueryAsync<RagChunk>(sql, new
        {
            Embedding = new Vector(queryEmbedding),
            Query = query,
            UserId = userId,
            TopK = topK
        });
    }

    public async Task<IEnumerable<RagChunk>> SearchByMetadataAsync(string key, string value, int userId)
    {
        using var conn = CreateConnection();
        const string sql = """
        SELECT 
            id,
            source,
            chunk_id AS ChunkId,
            start_index AS StartIndex,
            end_index AS EndIndex,
            content,
            is_public AS IsPublic,
            metadata::text AS Metadata,
            created_at AS CreatedAt,
            updated_at AS UpdatedAt
        FROM simp_rag
        WHERE (user_id = @UserId OR is_public = TRUE)
        AND metadata @> @Filter::jsonb
        ORDER BY updated_at DESC;
        """;
        return await conn.QueryAsync<RagChunk>(sql, new
        {
            UserId = userId,
            Filter = $"{{\"{key}\": \"{value}\"}}"
        });
    }
}