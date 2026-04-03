namespace NaviProject.Core.Models;

public class RagChunk
{
    public int Id { get; set; }
    public string Source { get; set; } = string.Empty;
    public string ChunkId { get; set; } = string.Empty;
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Content { get; set; } = string.Empty;
    public float[]? Embedding { get; set; }
    public bool IsPublic { get; set; }
    public string? Metadata { get; set; }
}