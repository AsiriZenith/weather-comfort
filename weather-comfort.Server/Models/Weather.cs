using System.Text.Json.Serialization;

namespace weather_comfort.Server.Models;

public class Weather
{
    [JsonPropertyName("main")]
    public MainData? Main { get; set; }

    [JsonPropertyName("wind")]
    public WindData? Wind { get; set; }

    [JsonPropertyName("clouds")]
    public CloudsData? Clouds { get; set; }

    [JsonPropertyName("weather")]
    public List<WeatherDescription>? WeatherDescriptions { get; set; }
}

public class MainData
{
    [JsonPropertyName("temp")]
    public double Temp { get; set; } 

    [JsonPropertyName("feels_like")]
    public double FeelsLike { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; } 
}

public class WindData
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; } 
}

public class CloudsData
{
    [JsonPropertyName("all")]
    public int All { get; set; } 
}

public class WeatherDescription
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

