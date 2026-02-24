using NaviProject.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviProject.Core.Interfaces
{
    public interface IRagRepository
    {
        Task InsertChunkAsync(RagChunk chunk);
        Task<IEnumerable<RagChunk>> SearchAsync(float[] queryEmbedding, int topK = 5);
        Task DeleteBySourceAsync(string source);
    }
}
