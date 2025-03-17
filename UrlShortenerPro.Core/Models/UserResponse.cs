namespace UrlShortenerPro.Core.Models;

public class UserResponse
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsPremium { get; set; }
    public System.DateTime CreatedAt { get; set; }
    public string? Token { get; set; }
}