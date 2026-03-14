using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaviProject.Api.Extensions;
using NaviProject.Core.Services;

namespace NaviProject.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RagController(RagService ragService) : ControllerBase
{
    [HttpPost("ingest")]
    public async Task<IActionResult> Ingest([FromBody] IngestRequest request)
    {
        var userId = User.GetUserId();
        await ragService.IngestTextAsync(request.Source, request.Text, userId);
        return Ok(new { message = "Ingested successfully" });
    }

    [HttpPost("ingest/public")]
    public async Task<IActionResult> IngestPublic([FromBody] IngestRequest request)
    {
        var userId = User.GetUserId();
        await ragService.IngestTextAsync(request.Source, request.Text, userId, isPublic: true);
        return Ok(new { message = "Ingested to public knowledge base successfully" });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int topK = 5)
    {
        var userId = User.GetUserId();
        var results = await ragService.SearchAsync(query, userId, topK);
        return Ok(results);
    }

    [HttpDelete("{source}")]
    public async Task<IActionResult> DeleteSource(string source)
    {
        var userId = User.GetUserId();
        await ragService.DeleteSourceAsync(source, userId);
        return NoContent();
    }
}

public record IngestRequest(string Source, string Text);