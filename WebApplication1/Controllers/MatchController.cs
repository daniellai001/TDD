using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchController(IMatchService matchService) : ControllerBase
{
    [HttpPost("UpdateMatchResult")]
    public async Task<ActionResult<UpdateMatchResultResponse>> UpdateMatchResult([FromBody] UpdateMatchResultRequest request)
    {
        var match = await matchService.UpdateMatchResultAsync(request.MatchId, request.MatchEvent);

        return new UpdateMatchResultResponse 
        { 
            DisplayResult = match.GetDisplayResult()
        };
    }
}