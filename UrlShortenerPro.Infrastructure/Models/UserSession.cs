using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UrlShortenerPro.Infrastructure.Models;

public class UserSession
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    [MaxLength(500)]
    public string? DeviceInfo { get; set; }
    
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    [MaxLength(100)]
    public string? Location { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(500)]
    public string? Token { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation property
    public User? User { get; set; }
} 