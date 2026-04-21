using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaviProject.Api.Extensions;
using NaviProject.Core.Interfaces;

namespace NaviProject.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfluenceController(IConfluenceService confluenceService) : ControllerBase
{
    [HttpGet("pages")]
    public async Task<IActionResult> GetPages()
    {
        var pages = await confluenceService.GetPagesAsync();
        return Ok(pages);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromBody] ConfluenceSyncRequest request)
    {
        var userId = User.GetUserId();
        var count = await confluenceService.SyncToKnowledgeBaseAsync(userId, request.IsPublic, request.MaxPages);
        return Ok(new { message = "Confluence pages synced successfully", pagesSynced = count });
    }

    public record ConfluenceSyncRequest(bool IsPublic = true, int MaxPages = 50);

}