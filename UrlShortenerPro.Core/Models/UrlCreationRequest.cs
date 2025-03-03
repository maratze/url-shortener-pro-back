namespace UrlShortenerPro.Core.Models;

public class UrlCreationRequest
{
    public string? OriginalUrl { get; set; }
    public string? CustomCode { get; set; }
    public int? ExpiresInDays { get; set; }
    public int? UserId { get; set; }
}