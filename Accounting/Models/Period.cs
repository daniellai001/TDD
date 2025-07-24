namespace Accounting.Models;

public class Period(DateTime from, DateTime to)
{
    public DateTime From { get; } = from;
    public DateTime To { get; } = to;

    public string GetFromYearMonth()
    {
        return from.ToString("yyyyMM");
    }

    public bool IsValid()
    {
        return From>To;
    }
}