using Microsoft.AspNetCore.Mvc;
using NaviProject.Core.Services;

namespace NaviProject.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RagController(RagService ragService) : ControllerBase
{
    [HttpPost("ingest")]
    public async Task<IActionResult> Ingest([FromBody] IngestRequest request)
    {
        await ragService.IngestTextAsync(request.Source, request.Text);
        return Ok(new { message = "Ingested successfully" });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int topK = 5)
    {
        var results = await ragService.SearchAsync(query, topK);
        return Ok(results);
    }

    [HttpDelete("{source}")]
    public async Task<IActionResult> DeleteSource(string source)
    {
        await ragService.DeleteSourceAsync(source);
        return NoContent();
    }
}

public record IngestRequest(string Source, string Text);