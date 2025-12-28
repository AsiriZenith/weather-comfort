namespace weather_comfort.Server.DTOs;

public class DashboardCityDto
{
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public double ComfortIndex { get; set; }
    public int Rank { get; set; }
}

