namespace NaviProject.Core.Models;

public class JiraTicket
{
    public string Key { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Assignee { get; set; } = string.Empty;
    public string Reporter { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<string> Comments { get; set; } = [];
    public bool HasAttachments { get; set; }
    public string Url { get; set; } = string.Empty;
}