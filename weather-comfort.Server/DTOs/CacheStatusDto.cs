namespace weather_comfort.Server.DTOs;

public class CacheStatusDto
{
    public int CityId { get; set; }
    public string Status { get; set; } = string.Empty; 
    public DateTime? CachedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

