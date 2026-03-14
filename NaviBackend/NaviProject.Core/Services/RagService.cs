using NaviProject.Core.Interfaces;
using NaviProject.Core.Models;

namespace NaviProject.Core.Services;

public class RagService(IRagRepository ragRepo, IEmbeddingService embeddingService)
{
    private const int ChunkSize = 5000;
    private const int ChunkOverlap = 50;

    public async Task IngestTextAsync(string source, string text)
    {
        var chunks = ChunkText(text);

        for (int i = 0; i < chunks.Count; i++)
        {
            var (content, start, end) = chunks[i];
            var embedding = await embeddingService.GetEmbeddingAsync(content);

            await ragRepo.InsertChunkAsync(new RagChunk
            {
                Source = source,
                ChunkId = $"{source}_{i}",
                StartIndex = start,
                EndIndex = end,
                Content = content,
                Embedding = embedding
            });
        }
    }

    public async Task<IEnumerable<RagChunk>> SearchAsync(string query, int topK = 5)
    {
        var embedding = await embeddingService.GetEmbeddingAsync(query);
        return await ragRepo.SearchAsync(embedding, topK);
    }

    public async Task DeleteSourceAsync(string source)
    {
        await ragRepo.DeleteBySourceAsync(source);
    }

    private static List<(string Content, int Start, int End)> ChunkText(string text)
    {
        var chunks = new List<(string, int, int)>();
        int start = 0;

        while (start < text.Length)
        {
            int end = Math.Min(start + ChunkSize, text.Length);
            chunks.Add((text[start..end], start, end));
            start += ChunkSize - ChunkOverlap;
        }

        return chunks;
    }
}