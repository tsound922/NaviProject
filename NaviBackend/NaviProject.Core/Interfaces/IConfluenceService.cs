using NaviProject.Core.Models;

namespace NaviProject.Core.Interfaces;

public interface IConfluenceService
{
    Task<List<ConfluencePage>> GetPagesAsync(int maxResults = 100);
    Task<int> SyncToKnowledgeBaseAsync(int userId, bool isPublic = true, int maxPages = 50);
}
