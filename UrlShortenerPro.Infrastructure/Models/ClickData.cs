namespace UrlShortenerPro.Infrastructure.Models;

public class ClickData
{
    public int Id { get; set; }
    public int UrlId { get; set; }
    public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? ReferrerUrl { get; set; }
        
    public Url? Url { get; set; }
}