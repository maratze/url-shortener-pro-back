namespace UrlShortenerPro.Core.Dtos;

public class UrlDto
{
    public int Id { get; set; }
    public string? OriginalUrl { get; set; }
    public string? ShortCode { get; set; }
    public int? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ClickCount { get; set; }
    public bool IsActive { get; set; } = true;
    public bool HasQrCode { get; set; }
} 