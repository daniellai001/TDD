using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IMatchRepository
{
    Task<Match?> GetMatchAsync(int matchId);
    Task<Match> SaveMatchAsync(Match match);
} 