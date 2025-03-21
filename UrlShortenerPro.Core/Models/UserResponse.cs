using System.Text.Json.Serialization;

namespace UrlShortenerPro.Core.Models;

public class UserResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }
    
    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }
    
    [JsonPropertyName("isPremium")]
    public bool IsPremium { get; set; }
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("lastLoginAt")]
    public DateTime? LastLoginAt { get; set; }
    
    [JsonPropertyName("role")]
    public string? Role { get; set; }
    
    [JsonPropertyName("token")]
    public string? Token { get; set; }
    
    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; set; }
    
    [JsonPropertyName("authProvider")]
    public string? AuthProvider { get; set; }
    
    [JsonPropertyName("isOAuthUser")]
    public bool? IsOAuthUser { get; set; }
    
    [JsonPropertyName("isTwoFactorEnabled")]
    public bool IsTwoFactorEnabled { get; set; }
    
    [JsonPropertyName("requiresTwoFactor")]
    public bool RequiresTwoFactor { get; set; }
}