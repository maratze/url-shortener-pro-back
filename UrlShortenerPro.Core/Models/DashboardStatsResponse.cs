using System.Text.Json.Serialization;

namespace UrlShortenerPro.Core.Models;

public class DashboardStatsResponse
{
    [JsonPropertyName("totalLinks")]
    public int TotalLinks { get; set; }
    
    [JsonPropertyName("linksWithQrCodes")]
    public int LinksWithQrCodes { get; set; }
    
    [JsonPropertyName("activeLinks")]
    public int ActiveLinks { get; set; }
    
    [JsonPropertyName("activeLinksPercentage")]
    public double ActiveLinksPercentage { get; set; }
    
    [JsonPropertyName("totalClicks")]
    public int TotalClicks { get; set; }
    
    [JsonPropertyName("newLinks")]
    public int NewLinks { get; set; }
    
    [JsonPropertyName("newQrCodes")]
    public int NewQrCodes { get; set; }
} 