using NaviProject.Core.Models;

namespace NaviProject.Core.Interfaces;

public interface IRagRepository
{
    Task InsertChunkAsync(RagChunk chunk, int userId, bool isPublic = false);
    Task<IEnumerable<RagChunk>> SearchAsync(float[] queryEmbedding, int userId, int topK = 5);
    Task<IEnumerable<RagChunk>> HybridSearchAsync(float[] queryEmbedding, string query, int userId, int topK = 5);
    Task<IEnumerable<RagChunk>> SearchByMetadataAsync(string key, string value, int userId);
    Task<DateTime?> GetLastSyncedAtAsync(string source);
    Task<HashSet<string>> GetSyncedSourcesAsync(string prefix);
    Task DeleteBySourceAsync(string source, int userId);
}