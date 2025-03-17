namespace UrlShortenerPro.Core.Dtos;

public class UserSessionDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public string? Token { get; set; }
    public bool IsActive { get; set; } = true;
} 