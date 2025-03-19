namespace UrlShortenerPro.Core.Models;

public class GoogleAuthRequest
{
    public string Code { get; set; }
    public string RedirectUri { get; set; }
}

public class GoogleTokenResponse
{
    public string Access_token { get; set; }
    public string Token_type { get; set; }
    public int Expires_in { get; set; }
    public string Refresh_token { get; set; }
    public string Id_token { get; set; }
}

public class GoogleUserInfo
{
    public string Id { get; set; }
    public string Email { get; set; }
    public bool Verified_email { get; set; }
    public string Name { get; set; }
    public string Given_name { get; set; }
    public string Family_name { get; set; }
    public string Picture { get; set; }
    public string Locale { get; set; }
} 