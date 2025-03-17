namespace UrlShortenerPro.Core.Dtos;

public class ClickDataDto
{
    public int Id { get; set; }
    public int UrlId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? ReferrerUrl { get; set; }
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
} 