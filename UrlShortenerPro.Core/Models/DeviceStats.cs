namespace UrlShortenerPro.Core.Models;

public class DeviceStats
{
    public Dictionary<string, int>? DeviceTypes { get; set; }
    public Dictionary<string, int>? Browsers { get; set; }
}