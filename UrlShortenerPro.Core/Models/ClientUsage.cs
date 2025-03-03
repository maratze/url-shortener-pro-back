namespace UrlShortenerPro.Core.Models;

public class ClientUsage
{
    public string? ClientId { get; set; }
    public int UsedRequests { get; set; }
    public DateTime LastRequestAt { get; set; }
}