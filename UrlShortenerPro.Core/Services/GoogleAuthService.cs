using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Models;

namespace UrlShortenerPro.Core.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleAuthService> _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public GoogleAuthService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GoogleAuthService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _clientId = _configuration["GoogleOAuth:ClientId"];
        _clientSecret = _configuration["GoogleOAuth:ClientSecret"];

        if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_clientSecret))
        {
            throw new InvalidOperationException("Google OAuth client ID or client secret is not configured");
        }
    }

    public string GetAuthorizationUrl(string redirectUri, string state = null)
    {
        var googleAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        
        var queryParams = new Dictionary<string, string>
        {
            { "client_id", _clientId },
            { "redirect_uri", redirectUri },
            { "response_type", "code" },
            { "scope", "email profile openid" },
            { "access_type", "offline" },
            { "prompt", "consent" }
        };
        
        if (!string.IsNullOrEmpty(state))
        {
            queryParams.Add("state", state);
        }
        
        var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
        return $"{googleAuthUrl}?{queryString}";
    }

    public async Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri)
    {
        try
        {
            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            
            var requestData = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            };
            
            var content = new FormUrlEncodedContent(requestData);
            
            var response = await _httpClient.PostAsync(tokenEndpoint, content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging code for token");
            throw new InvalidOperationException("Failed to exchange authorization code for token", ex);
        }
    }

    public async Task<GoogleUserInfo> GetUserInfoAsync(string accessToken)
    {
        try
        {
            var userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await _httpClient.GetAsync(userInfoEndpoint);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user info");
            throw new InvalidOperationException("Failed to retrieve user information", ex);
        }
    }
} 