using NaviProject.Core.Models;

namespace NaviProject.Core.Interfaces;

public interface IRagRepository
{
    Task InsertChunkAsync(RagChunk chunk, int userId, bool isPublic = false);
    Task<IEnumerable<RagChunk>> SearchAsync(float[] queryEmbedding, int userId, int topK = 5);
    Task DeleteBySourceAsync(string source, int userId);
}