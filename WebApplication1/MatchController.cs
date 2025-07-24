using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1;

[ApiController]
[Route("api/[controller]")]
public class MatchController : ControllerBase
{
    private readonly IMatchRepository _matchRepository;

    public MatchController(IMatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    [HttpPost("UpdateMatchResult")]
    public async Task<ActionResult<UpdateMatchResultResponse>> UpdateMatchResult([FromBody] UpdateMatchResultRequest request)
    {
        var match = await _matchRepository.UpdateMatchAsync(request.MatchId, request.MatchEvent);
        

        return new UpdateMatchResultResponse 
        { 
            DisplayResult = match.GetDisplayResult()
        };
    }
}