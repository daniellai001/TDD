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

    public Task<Match> UpdateMatchAsync(int matchId, MatchEvent matchEvent)
    {
        var match = _matches.GetValueOrDefault(matchId) ?? new Match { MatchId = matchId };

        switch (matchEvent)
        {
            case MatchEvent.HomeGoal:
                match.MatchResult += "H";
                break;
            case MatchEvent.AwayGoal:
                match.MatchResult += "A";
                break;
            case MatchEvent.NextPeriod:
                // 添加分號標記進入下半場
                if (!match.MatchResult.Contains(';'))
                {
                    match.MatchResult += ";";
                }
                break;
            case MatchEvent.HomeCancel:
                // 檢查去除分號後最後一個字符是否是 'H'
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
                // 檢查去除分號後最後一個字符是否是 'A'
                var resultWithoutSemicolonAway = match.MatchResult.Replace(";", "");
                if (resultWithoutSemicolonAway.Length > 0 && resultWithoutSemicolonAway[resultWithoutSemicolonAway.Length - 1] == 'A')
                {
                    var lastAIndex = match.MatchResult.LastIndexOf('A');
                    match.MatchResult = match.MatchResult.Remove(lastAIndex, 1);
                }
                else
                {
                    throw new UpdateMatchResultException(matchId, matchEvent, match.MatchResult);
                }
                break;
        }

        _matches[matchId] = match;
        return Task.FromResult(match);
    }
} 