namespace UrlShortenerPro.Core.Models;

public class ClickStats
{
    public int TotalClicks { get; set; }
    public Dictionary<DateTime, int>? ClicksByDate { get; set; }
}