namespace UrlShortenerPro.Api.DTOs;

public class CreateUrlRequest
{
    public string? OriginalUrl { get; set; }
    public string? CustomCode { get; set; }
    public int? ExpiresInDays { get; set; }
}