namespace UrlShortenerPro.Core.Models;

public class LocationStats
{
    public Dictionary<string, int>? Countries { get; set; }
    public Dictionary<string, int>? Cities { get; set; }
}