namespace weather_comfort.Server.DTOs;

public class ComfortIndexDto
{
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public double Score { get; set; }
    public double? TemperaturePenalty { get; set; }
    public double? HumidityPenalty { get; set; }
    public double? WindPenalty { get; set; }
    public double? CloudinessPenalty { get; set; }
}

