using NaviProject.Core.Interfaces;
using NaviProject.Core.Models;
using NaviProject.Core.Services;
using NaviProject.Infrastructure.Repositories;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NaviProject.Api.Services;

public class JiraService(IConfiguration configuration, RagService ragService, IRagRepository ragRepository) : IJiraService
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

    public async Task<List<JiraTicket>> GetMyTicketsAsync(int maxResults = 100)
    {
        using var client = CreateClient();
        var baseUrl = configuration["Jira:BaseUrl"]!;
        var tickets = new List<JiraTicket>();
        string? nextPageToken = null;
        bool isLast = false;

        while (!isLast)
        {
            var jql = Uri.EscapeDataString("assignee = currentUser() OR reporter = currentUser() ORDER BY updated DESC");
            var url = $"{baseUrl}/rest/api/3/search/jql?jql={jql}&maxResults=50&fields=summary,description,status,assignee,reporter,priority,created,updated,comment,attachment";
            if (nextPageToken != null)
                url += $"&nextPageToken={Uri.EscapeDataString(nextPageToken)}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var issues = root.GetProperty("issues");
            isLast = root.TryGetProperty("isLast", out var isLastProp) && isLastProp.GetBoolean();
            nextPageToken = root.TryGetProperty("nextPageToken", out var tokenProp)
                ? tokenProp.GetString() : null;

            foreach (var issue in issues.EnumerateArray())
            {
                var fields = issue.GetProperty("fields");
                var ticket = new JiraTicket
                {
                    Key = issue.GetProperty("key").GetString() ?? string.Empty,
                    Summary = GetStringOrEmpty(fields, "summary"),
                    Status = fields.TryGetProperty("status", out var status) && status.ValueKind != JsonValueKind.Null
                        ? GetStringOrEmpty(status, "name") : string.Empty,
                    Assignee = fields.TryGetProperty("assignee", out var assignee) && assignee.ValueKind != JsonValueKind.Null
                        ? GetStringOrEmpty(assignee, "displayName") : "Unassigned",
                    Reporter = fields.TryGetProperty("reporter", out var reporter) && reporter.ValueKind != JsonValueKind.Null
                        ? GetStringOrEmpty(reporter, "displayName") : string.Empty,
                    Priority = fields.TryGetProperty("priority", out var priority) && priority.ValueKind != JsonValueKind.Null
                        ? GetStringOrEmpty(priority, "name") : string.Empty,
                    Description = ExtractDescription(fields),
                    Comments = ExtractComments(fields),
                    HasAttachments = fields.TryGetProperty("attachment", out var attachments)
                            && attachments.ValueKind == JsonValueKind.Array
                            && attachments.GetArrayLength() > 0,
                    Url = $"{configuration["Jira:BaseUrl"]}/browse/{issue.GetProperty("key").GetString()}"
                };

                if (fields.TryGetProperty("created", out var created))
                    ticket.CreatedAt = DateTimeOffset.Parse(created.GetString()!, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None).UtcDateTime;
                if (fields.TryGetProperty("updated", out var updated))
                    ticket.UpdatedAt = DateTimeOffset.Parse(updated.GetString()!, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None).UtcDateTime;

                tickets.Add(ticket);
            }

            if (!isLast) await Task.Delay(500);
        }

        return tickets;
    }
    public async Task SyncToKnowledgeBaseAsync(int userId, bool isPublic = false)
    {
        var tickets = await GetMyTicketsAsync();

        foreach (var ticket in tickets)
        {
            var content = BuildTicketContent(ticket);
            var metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                type = "jira",
                ticket_key = ticket.Key,
                url = ticket.Url,
                assignee = ticket.Assignee,
                reporter = ticket.Reporter,
                status = ticket.Status,
                priority = ticket.Priority,
                has_attachments = ticket.HasAttachments,
                created_at = ticket.CreatedAt?.ToString("yyyy-MM-dd"),
                updated_at = ticket.UpdatedAt?.ToString("yyyy-MM-dd")
            });

            var chunks = new List<(string content, int start, int end)>();
            var chunkSize = 500;
            var overlap = 50;
            int start = 0;
            while (start < content.Length)
            {
                int end = Math.Min(start + chunkSize, content.Length);
                chunks.Add((content[start..end], start, end));
                start += chunkSize - overlap;
            }

            for (int i = 0; i < chunks.Count; i++)
            {
                var (chunkContent, chunkStart, chunkEnd) = chunks[i];
                var embedding = await ragService.GetEmbeddingForChunkAsync(chunkContent);

                await ragRepository.InsertChunkAsync(new RagChunk
                {
                    Source = $"jira:{ticket.Key}",
                    ChunkId = $"jira:{ticket.Key}_{i}",
                    StartIndex = chunkStart,
                    EndIndex = chunkEnd,
                    Content = chunkContent,
                    Embedding = embedding,
                    Metadata = metadata
                }, userId, isPublic);

                await Task.Delay(200);
            }

            await Task.Delay(200);
        }
    }
    private static string BuildTicketContent(JiraTicket ticket)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Jira Ticket: {ticket.Key}");
        sb.AppendLine($"Summary: {ticket.Summary}");
        sb.AppendLine($"Status: {ticket.Status}");
        sb.AppendLine($"Priority: {ticket.Priority}");
        sb.AppendLine($"Assignee: {ticket.Assignee}");
        sb.AppendLine($"Reporter: {ticket.Reporter}");

        if (ticket.CreatedAt.HasValue)
            sb.AppendLine($"Created: {ticket.CreatedAt:yyyy-MM-dd}");
        if (ticket.UpdatedAt.HasValue)
            sb.AppendLine($"Updated: {ticket.UpdatedAt:yyyy-MM-dd}");

        if (!string.IsNullOrEmpty(ticket.Description))
        {
            sb.AppendLine();
            sb.AppendLine("Description:");
            sb.AppendLine(ticket.Description);
        }

        if (ticket.Comments.Any())
        {
            sb.AppendLine();
            sb.AppendLine("Comments:");
            foreach (var comment in ticket.Comments)
            {
                sb.AppendLine($"- {comment}");
            }
        }

        return sb.ToString();
    }

    private static string ExtractDescription(JsonElement fields)
    {
        if (!fields.TryGetProperty("description", out var desc) || desc.ValueKind == JsonValueKind.Null)
            return string.Empty;

        // Jira Cloud is using Atlassian Document Format (ADF)
        return ExtractTextFromAdf(desc);
    }

    private static string ExtractTextFromAdf(JsonElement node)
    {
        var sb = new StringBuilder();

        if (node.ValueKind == JsonValueKind.Object)
        {
            if (node.TryGetProperty("text", out var text))
                sb.Append(text.GetString());

            if (node.TryGetProperty("content", out var content))
                foreach (var child in content.EnumerateArray())
                    sb.Append(ExtractTextFromAdf(child));
        }

        return sb.ToString();
    }

    private static List<string> ExtractComments(JsonElement fields)
    {
        var comments = new List<string>();

        if (!fields.TryGetProperty("comment", out var commentField))
            return comments;

        if (!commentField.TryGetProperty("comments", out var commentList))
            return comments;

        foreach (var comment in commentList.EnumerateArray())
        {
            if (comment.TryGetProperty("body", out var body))
            {
                var text = ExtractTextFromAdf(body);
                if (!string.IsNullOrWhiteSpace(text))
                    comments.Add(text.Trim());
            }
        }

        return comments;
    }

    private static string GetStringOrEmpty(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }
}