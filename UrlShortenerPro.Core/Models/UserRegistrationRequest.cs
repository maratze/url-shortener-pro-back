namespace UrlShortenerPro.Core.Models;

public class UserRegistrationRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
}