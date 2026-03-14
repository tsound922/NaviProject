using NaviProject.Core.Interfaces;
using OllamaSharp;

namespace NaviProject.Api.Services;

public class OllamaEmbeddingService(OllamaApiClient ollamaClient) : IEmbeddingService
{
    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var result = await ollamaClient.EmbedAsync(new OllamaSharp.Models.EmbedRequest
        {
            Model = "nomic-embed-text",
            Input = new List<string> { text }
        });

        return result.Embeddings![0];
    }
}