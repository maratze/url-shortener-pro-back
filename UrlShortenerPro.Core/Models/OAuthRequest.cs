namespace UrlShortenerPro.Core.Models;

public class OAuthRequest
{
    public string? Provider { get; set; }
    public string? Token { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Picture { get; set; }
} 