namespace UrlShortenerPro.Core.Models;

public class ChangePasswordRequest
{
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
    public bool? IsGoogleUser { get; set; }
} 