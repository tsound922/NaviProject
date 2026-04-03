using NaviProject.Core.Interfaces;
using NaviProject.Core.Models;
using System.Text.RegularExpressions;

namespace NaviProject.Core.Services;

public class RagService(IRagRepository ragRepo, IEmbeddingService embeddingService)
{
    private const int ChunkSize = 500;
    private const int ChunkOverlap = 50;

    public async Task IngestTextAsync(string source, string text, int userId, bool isPublic = false)
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
            }, userId, isPublic);
        }
    }

    public async Task<IEnumerable<RagChunk>> SearchAsync(string query, int userId, int topK = 5)
    {
        // pick up ticket number from query
        var ticketKey = ExtractTicketKey(query);
        if (ticketKey != null)
        {
            return await ragRepo.SearchByMetadataAsync("ticket_key", ticketKey, userId);
        }

        // use hybrid searching 
        var embedding = await embeddingService.GetEmbeddingAsync(query);
        return await ragRepo.HybridSearchAsync(embedding, query, userId, topK);
    }

    public async Task<IEnumerable<RagChunk>> SearchByMetadataAsync(string key, string value, int userId)
    {
        return await ragRepo.SearchByMetadataAsync(key, value, userId);
    }

    public async Task DeleteSourceAsync(string source, int userId)
    {
        await ragRepo.DeleteBySourceAsync(source, userId);
    }

    public async Task<float[]> GetEmbeddingForChunkAsync(string text)
    {
        return await embeddingService.GetEmbeddingAsync(text);
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
    private static string? ExtractTicketKey(string query)
    {
        var match = Regex.Match(query, @"\b([A-Z]+-\d+)\b", RegexOptions.IgnoreCase);
        return match.Success ? match.Value.ToUpper() : null;
    }

}