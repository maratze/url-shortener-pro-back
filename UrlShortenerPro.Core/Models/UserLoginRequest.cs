namespace UrlShortenerPro.Core.Models;

public class UserLoginRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool Remember { get; set; } = false;
    public string? VerificationCode { get; set; }
}