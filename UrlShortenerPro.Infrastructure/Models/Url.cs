using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UrlShortenerPro.Infrastructure.Models;

public class Url
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(2048)]
    public string? OriginalUrl { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string? ShortCode { get; set; }
    
    [ForeignKey("User")]
    public int? UserId { get; set; }
    
    public User? User { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpiresAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int ClickCount { get; set; } = 0;
    
    public ICollection<ClickData>? ClickData { get; set; }
}