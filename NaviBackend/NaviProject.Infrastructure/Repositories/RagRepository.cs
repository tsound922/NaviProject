using Dapper;
using NaviProject.Core.Interfaces;
using NaviProject.Core.Models;
using Npgsql;
using Pgvector;
using Pgvector.Dapper;

namespace NaviProject.Infrastructure.Repositories;

public class RagRepository(string connectionString) : IRagRepository
{
    private NpgsqlConnection CreateConnection()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseVector();
        var dataSource = dataSourceBuilder.Build();
        return dataSource.OpenConnection();
    }

    public async Task InsertChunkAsync(RagChunk chunk)
    {
        using var conn = CreateConnection();
        const string sql = """
            INSERT INTO simp_rag (source, chunk_id, start_index, end_index, content, embedding)
            VALUES (@Source, @ChunkId, @StartIndex, @EndIndex, @Content, @Embedding)
            ON CONFLICT (source, chunk_id) DO UPDATE
            SET content = EXCLUDED.content,
                embedding = EXCLUDED.embedding,
                updated_at = NOW();
            """;
        await conn.ExecuteAsync(sql, new
        {
            chunk.Source,
            chunk.ChunkId,
            chunk.StartIndex,
            chunk.EndIndex,
            chunk.Content,
            Embedding = chunk.Embedding != null ? new Vector(chunk.Embedding) : null
        });
    }

   public async Task<IEnumerable<RagChunk>> SearchAsync(float[] queryEmbedding, int topK = 5)
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
            created_at AS CreatedAt,
            updated_at AS UpdatedAt
        FROM simp_rag
        ORDER BY embedding <=> @Embedding
        LIMIT @TopK;
        """;
    return await conn.QueryAsync<RagChunk>(sql, new
    {
        Embedding = new Vector(queryEmbedding),
        TopK = topK
    });
}

    public async Task DeleteBySourceAsync(string source)
    {
        using var conn = CreateConnection();
        const string sql = "DELETE FROM simp_rag WHERE source = @Source;";
        await conn.ExecuteAsync(sql, new { Source = source });
    }
}