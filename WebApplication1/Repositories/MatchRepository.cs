using WebApplication1.Models;

namespace WebApplication1.Repositories;

public class MatchRepository : IMatchRepository
{
    private readonly Dictionary<int, Match> _matches = new();

    public Task<Match?> GetMatchAsync(int matchId)
    {
        _matches.TryGetValue(matchId, out var match);
        return Task.FromResult(match);
    }

    public Task<Match> SaveMatchAsync(Match match)
    {
        _matches[match.MatchId] = match;
        return Task.FromResult(match);
    }
} 