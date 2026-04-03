using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaviProject.Api.Extensions;
using NaviProject.Core.Interfaces;

namespace NaviProject.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JiraController(IJiraService jiraService) : ControllerBase
{
    [HttpGet("tickets")]
    public async Task<IActionResult> GetTickets()
    {
        var tickets = await jiraService.GetMyTicketsAsync();
        return Ok(tickets);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromBody] SyncRequest request)
    {
        var userId = User.GetUserId();
        await jiraService.SyncToKnowledgeBaseAsync(userId, request.IsPublic);
        return Ok(new { message = "Jira tickets synced successfully" });
    }
}

public record SyncRequest(bool IsPublic = false);