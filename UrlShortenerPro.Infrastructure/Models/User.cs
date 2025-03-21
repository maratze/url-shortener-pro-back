using System.ComponentModel.DataAnnotations;

namespace UrlShortenerPro.Infrastructure.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }
    
    [Required]
    public string? PasswordHash { get; set; }
    
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    public bool IsPremium { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    public ICollection<Url>? Urls { get; set; }

    public ICollection<UserSession>? Sessions { get; set; }
    
    [MaxLength(50)]
    public string? Role { get; set; } = "User";
    
    [MaxLength(50)]
    public string? AuthProvider { get; set; } = "Local";
    
    /// <summary>
    /// Включена ли двухфакторная аутентификация
    /// </summary>
    public bool IsTwoFactorEnabled { get; set; } = false;
    
    /// <summary>
    /// Секретный ключ для двухфакторной аутентификации
    /// </summary>
    [MaxLength(100)]
    public string? TwoFactorSecret { get; set; }
}