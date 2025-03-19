using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Core.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri);
    Task<GoogleUserInfo> GetUserInfoAsync(string accessToken);
    string GetAuthorizationUrl(string redirectUri, string state = null);
} 