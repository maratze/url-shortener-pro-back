namespace UrlShortenerPro.Core.Models;

public class ClickTrackingData
{
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? ReferrerUrl { get; set; }
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
} 