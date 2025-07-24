namespace WebApplication1.Models;

public class UpdateMatchResultException : Exception
{
    public int MatchId { get; }
    public MatchEvent MatchEvent { get; }
    public string MatchResult { get; }

    public UpdateMatchResultException(int matchId, MatchEvent matchEvent, string matchResult)
        : base($"{matchId}, {matchEvent} + {matchResult}")
    {
        MatchId = matchId;
        MatchEvent = matchEvent;
        MatchResult = matchResult;
    }

    public UpdateMatchResultException(int matchId, MatchEvent matchEvent, string matchResult, string message)
        : base(message)
    {
        MatchId = matchId;
        MatchEvent = matchEvent;
        MatchResult = matchResult;
    }
} 