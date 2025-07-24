namespace WebApplication1.Models;

public class Match
{
    public int MatchId { get; set; }
    public string MatchResult { get; set; } = string.Empty;

    public string GetDisplayResult()
    {
        var homeScore = MatchResult.Count(x => x == 'H');
        var awayScore = MatchResult.Count(x => x == 'A');
        var isSecondHalf = MatchResult.Contains(';');
        return $"{homeScore}:{awayScore} ({(isSecondHalf ? "Second" : "First")} Half)";
    }
} 