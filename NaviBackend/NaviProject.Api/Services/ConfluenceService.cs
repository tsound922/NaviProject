using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using NaviProject.Core.Interfaces;
using NaviProject.Core.Models;
using NaviProject.Core.Services;

namespace NaviProject.Api.Services;

public class ConfluenceService(IConfiguration configuration, RagService ragService) : IConfluenceService
{
    private HttpClient CreateClient()
    {
        var baseUrl = configuration["Jira:BaseUrl"]!;
        var email = configuration["Jira:Email"]!;
        var apiToken = configuration["Jira:ApiToken"]!;

        var client = new HttpClient();
        client.BaseAddress = new Uri(baseUrl);
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{apiToken}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    public async Task<List<ConfluencePage>> GetPagesAsync(int maxResults = 10)
    {
        using var client = CreateClient();
        var baseUrl = configuration["Jira:BaseUrl"]!;
        var pages = new List<ConfluencePage>();
        int start = 0;

        while (pages.Count < maxResults)
        {
            var batchSize = Math.Min(50, maxResults - pages.Count);
            var url = $"{baseUrl}/wiki/rest/api/content?type=page&start={start}&limit={batchSize}&expand=body.storage,space,version";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var results = root.GetProperty("results");
            var size = root.GetProperty("size").GetInt32();

            foreach (var result in results.EnumerateArray())
            {
                if (pages.Count >= maxResults) break;

                var page = new ConfluencePage
                {
                    Id = result.GetProperty("id").GetString() ?? string.Empty,
                    Title = result.GetProperty("title").GetString() ?? string.Empty,
                    SpaceKey = result.TryGetProperty("space", out var space)
                        ? space.GetProperty("key").GetString() ?? string.Empty : string.Empty,
                    SpaceName = result.TryGetProperty("space", out var spaceInfo)
                        ? spaceInfo.GetProperty("name").GetString() ?? string.Empty : string.Empty,
                    Url = $"{baseUrl}/wiki{result.GetProperty("_links").GetProperty("webui").GetString()}",
                    Content = ExtractContent(result),
                    LastModifiedBy = ExtractLastModifiedBy(result),
                    LastModifiedAt = ExtractLastModifiedAt(result),
                };

                pages.Add(page);
            }

            start += size;
            if (size < batchSize) break;

            if (pages.Count < maxResults) await Task.Delay(500);
        }

        return pages;
    }

    public async Task<int> SyncToKnowledgeBaseAsync(int userId, bool isPublic = true, int maxPages = 50)
    {
        var processedCount = 0;
        int start = 0;
        bool hasMore = true;
        using var client = CreateClient();
        var baseUrl = configuration["Jira:BaseUrl"]!;

        var syncedSources = await ragService.GetSyncedSourcesAsync("confluence:");

        while (hasMore && (maxPages <= 0 || processedCount < maxPages))
        {
            var batchSize = 50;
            var url = $"{baseUrl}/wiki/rest/api/content?type=page&start={start}&limit={batchSize}&expand=body.storage,space,version";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var results = root.GetProperty("results");
            var size = root.GetProperty("size").GetInt32();

            foreach (var result in results.EnumerateArray())
            {
                if (maxPages > 0 && processedCount >= maxPages) break;

                var id = result.GetProperty("id").GetString() ?? string.Empty;
                var source = $"confluence:{id}";

                // check if already synced and not updated, skip
                var lastModifiedAt = ExtractLastModifiedAt(result);

                // skip if we have a synced version and the page has not been modified since last sync
                if (syncedSources.Contains(source)) continue;

                var content = ExtractContent(result);
                if (string.IsNullOrWhiteSpace(content)) continue;

                var title = result.GetProperty("title").GetString() ?? string.Empty;
                var spaceKey = result.TryGetProperty("space", out var space)
                    ? space.GetProperty("key").GetString() ?? string.Empty : string.Empty;
                var spaceName = result.TryGetProperty("space", out var spaceInfo)
                    ? spaceInfo.GetProperty("name").GetString() ?? string.Empty : string.Empty;
                var pageUrl = $"{baseUrl}/wiki{result.GetProperty("_links").GetProperty("webui").GetString()}";
                var lastModifiedBy = ExtractLastModifiedBy(result);

                var metadata = JsonSerializer.Serialize(new
                {
                    type = "confluence",
                    page_id = id,
                    title,
                    url = pageUrl,
                    space_key = spaceKey,
                    space_name = spaceName,
                    last_modified_by = lastModifiedBy,
                    last_modified_at = lastModifiedAt?.ToString("yyyy-MM-dd")
                });

                await ragService.IngestTextAsync(
                    source,
                    $"Title: {title}\nSpace: {spaceName}\n\n{content}",
                    userId,
                    isPublic,
                    metadata);

                processedCount++;
                await Task.Delay(200);
            }

            start += size;
            hasMore = size == batchSize;

            if (hasMore) await Task.Delay(500);
        }

        return processedCount;
    }
    private static string ExtractContent(JsonElement result)
    {
        if (!result.TryGetProperty("body", out var body)) return string.Empty;
        if (!body.TryGetProperty("storage", out var storage)) return string.Empty;
        if (!storage.TryGetProperty("value", out var value)) return string.Empty;

        var html = value.GetString() ?? string.Empty;
        return StripHtml(html);
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;

        //Remove HTML tags
        var text = Regex.Replace(html, "<[^>]+>", " ");
        // remove multiple spaces and newlines
        text = Regex.Replace(text, @"\s+", " ");
        // decode HTML entities
        text = text.Replace("&nbsp;", " ")
                   .Replace("&amp;", "&")
                   .Replace("&lt;", "<")
                   .Replace("&gt;", ">")
                   .Replace("&quot;", "\"");
        return text.Trim();
    }

    private static string ExtractLastModifiedBy(JsonElement result)
    {
        if (!result.TryGetProperty("version", out var version)) return string.Empty;
        if (!version.TryGetProperty("by", out var by)) return string.Empty;
        return by.TryGetProperty("displayName", out var name)
            ? name.GetString() ?? string.Empty : string.Empty;
    }

    private static DateTime? ExtractLastModifiedAt(JsonElement result)
    {
        if (!result.TryGetProperty("version", out var version)) return null;
        if (!version.TryGetProperty("when", out var when)) return null;
        return DateTimeOffset.Parse(when.GetString()!,
            System.Globalization.CultureInfo.InvariantCulture).UtcDateTime;
    }
}