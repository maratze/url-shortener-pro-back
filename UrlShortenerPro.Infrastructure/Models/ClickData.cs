using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UrlShortenerPro.Infrastructure.Models;

public class ClickData
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("Url")]
    public int UrlId { get; set; }
    
    public Url? Url { get; set; }
    
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    [MaxLength(1024)]
    public string? ReferrerUrl { get; set; }
    
    [MaxLength(50)]
    public string? DeviceType { get; set; }
    
    [MaxLength(50)]
    public string? Browser { get; set; }
    
    [MaxLength(50)]
    public string? OperatingSystem { get; set; }
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
}