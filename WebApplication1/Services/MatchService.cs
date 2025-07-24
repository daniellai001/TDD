using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public class MatchService(IMatchRepository matchRepository) : IMatchService
{
    public async Task<Match?> GetMatchAsync(int matchId)
    {
        return await matchRepository.GetMatchAsync(matchId);
    }

    public async Task<Match> UpdateMatchResultAsync(int matchId, MatchEvent matchEvent)
    {
        var match = await matchRepository.GetMatchAsync(matchId) ?? new Match { MatchId = matchId };

        switch (matchEvent)
        {
            case MatchEvent.HomeGoal:
                match.MatchResult += "H";
                break;
            case MatchEvent.AwayGoal:
                match.MatchResult += "A";
                break;
            case MatchEvent.NextPeriod:
                if (!match.MatchResult.Contains(';'))
                {
                    match.MatchResult += ";";
                }
                break;
            case MatchEvent.HomeCancel:
                var resultWithoutSemicolon = match.MatchResult.Replace(";", "");
                if (resultWithoutSemicolon.Length > 0 && resultWithoutSemicolon[resultWithoutSemicolon.Length - 1] == 'H')
                {
                    var lastHIndex = match.MatchResult.LastIndexOf('H');
                    match.MatchResult = match.MatchResult.Remove(lastHIndex, 1);
                }
                else
                {
                    throw new UpdateMatchResultException(matchId, matchEvent, match.MatchResult);
                }
                break;
            case MatchEvent.AwayCancel:
                var resultWithoutSemicolonAway = match.MatchResult.Replace(";", "");
                if (resultWithoutSemicolonAway.Length > 0 && resultWithoutSemicolonAway[^1] == 'A')
                {
                    var lastAIndex = match.MatchResult.LastIndexOf('A');
                    match.MatchResult = match.MatchResult.Remove(lastAIndex, 1);
                }
                else
                {
                    throw new UpdateMatchResultException(matchId, matchEvent, match.MatchResult);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(matchEvent), matchEvent, null);
        }

        return await matchRepository.SaveMatchAsync(match);
    }
} 