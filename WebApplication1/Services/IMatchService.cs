using WebApplication1.Models;

namespace WebApplication1.Services;

public interface IMatchService
{
    Task<Match?> GetMatchAsync(int matchId);
    Task<Match> UpdateMatchResultAsync(int matchId, MatchEvent matchEvent);
} 