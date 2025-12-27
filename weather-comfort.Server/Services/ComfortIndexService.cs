using Microsoft.Extensions.Logging;
using weather_comfort.Server.DTOs;

namespace weather_comfort.Server.Services;

public class ComfortIndexService : IComfortIndexService
{
    // Ideal conditions
    private const double IdealTemperature = 22.0; 
    private const int IdealHumidityMin = 40; 
    private const int IdealHumidityMax = 60;
    private const double IdealWindSpeed = 5.0; 
    private const int IdealCloudiness = 30; 

    // Penalty weights (points per unit deviation)
    private const double TemperaturePenaltyWeight = 1.5; 
    private const double HumidityPenaltyWeight = 0.5;
    private const double WindPenaltyWeight = 2.0;
    private const double CloudinessPenaltyWeight = 0.3;

    private const double BaseScore = 100.0;
    private const double MinScore = 0.0;
    private const double MaxScore = 100.0;

    private readonly ILogger<ComfortIndexService> _logger;

    public ComfortIndexService(ILogger<ComfortIndexService> logger)
    {
        _logger = logger;
    }

    public ComfortIndexDto CalculateComfortIndex(WeatherDto weather)
    {
        if (weather == null)
        {
            _logger.LogWarning("Null weather data provided for Comfort Index calculation");
            return CreateDefaultComfortIndex(0, "Unknown");
        }

        try
        {
            // Validate and handle missing/invalid data
            var temperature = ValidateTemperature(weather.Temperature);
            var humidity = ValidateHumidity(weather.Humidity);
            var windSpeed = ValidateWindSpeed(weather.WindSpeed);
            var cloudiness = ValidateCloudiness(weather.Cloudiness);

            // Calculate penalties
            var temperaturePenalty = CalculateTemperaturePenalty(temperature);
            var humidityPenalty = CalculateHumidityPenalty(humidity);
            var windPenalty = CalculateWindPenalty(windSpeed);
            var cloudinessPenalty = CalculateCloudinessPenalty(cloudiness);

            // Calculate final score
            var score = BaseScore - temperaturePenalty - humidityPenalty - windPenalty - cloudinessPenalty;
            score = Math.Max(MinScore, Math.Min(MaxScore, score));

            return new ComfortIndexDto
            {
                CityId = weather.CityId,
                CityName = weather.CityName,
                Score = Math.Round(score, 2),
                TemperaturePenalty = Math.Round(temperaturePenalty, 2),
                HumidityPenalty = Math.Round(humidityPenalty, 2),
                WindPenalty = Math.Round(windPenalty, 2),
                CloudinessPenalty = Math.Round(cloudinessPenalty, 2)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Comfort Index for city {CityId} ({CityName})", 
                weather.CityId, weather.CityName);
            return CreateDefaultComfortIndex(weather.CityId, weather.CityName);
        }
    }

    public IReadOnlyList<ComfortIndexDto> CalculateComfortIndexForAll(IReadOnlyList<WeatherDto> weatherList)
    {
        if (weatherList == null || weatherList.Count == 0)
        {
            _logger.LogWarning("Empty or null weather list provided for Comfort Index calculation");
            return Array.Empty<ComfortIndexDto>().ToList().AsReadOnly();
        }

        var results = new List<ComfortIndexDto>();

        foreach (var weather in weatherList)
        {
            try
            {
                var comfortIndex = CalculateComfortIndex(weather);
                results.Add(comfortIndex);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to calculate Comfort Index for city {CityId} ({CityName}). Skipping.", 
                    weather?.CityId ?? 0, weather?.CityName ?? "Unknown");
            }
        }

        return results.AsReadOnly();
    }

    private double CalculateTemperaturePenalty(double temperature)
    {
        var deviation = Math.Abs(temperature - IdealTemperature);
        return deviation * TemperaturePenaltyWeight;
    }

    private double CalculateHumidityPenalty(int humidity)
    {
        if (humidity >= IdealHumidityMin && humidity <= IdealHumidityMax)
        {
            return 0;
        }

        // Calculate penalty based on distance from ideal range
        var deviation = humidity < IdealHumidityMin 
            ? IdealHumidityMin - humidity 
            : humidity - IdealHumidityMax;
        
        return deviation * HumidityPenaltyWeight;
    }

    private double CalculateWindPenalty(double windSpeed)
    {
        if (windSpeed <= IdealWindSpeed)
        {
            return 0;
        }

        var deviation = windSpeed - IdealWindSpeed;
        return deviation * WindPenaltyWeight;
    }

    private double CalculateCloudinessPenalty(int cloudiness)
    {
        if (cloudiness <= IdealCloudiness)
        {
            return 0;
        }

        var deviation = cloudiness - IdealCloudiness;
        return deviation * CloudinessPenaltyWeight;
    }

    private double ValidateTemperature(double temperature)
    {
        // Handle extreme but valid temperatures (-50°C to 60°C)
        if (double.IsNaN(temperature) || double.IsInfinity(temperature))
        {
            _logger.LogWarning("Invalid temperature value: {Temperature}. Using default 22°C", temperature);
            return IdealTemperature;
        }
        return temperature;
    }

    private int ValidateHumidity(int humidity)
    {
        // Humidity should be 0-100%
        if (humidity < 0 || humidity > 100)
        {
            _logger.LogWarning("Invalid humidity value: {Humidity}. Clamping to valid range", humidity);
            return Math.Max(0, Math.Min(100, humidity));
        }
        return humidity;
    }

    private double ValidateWindSpeed(double windSpeed)
    {
        // Wind speed should be non-negative
        if (windSpeed < 0 || double.IsNaN(windSpeed) || double.IsInfinity(windSpeed))
        {
            _logger.LogWarning("Invalid wind speed value: {WindSpeed}. Using 0", windSpeed);
            return 0;
        }
        return windSpeed;
    }

    private int ValidateCloudiness(int cloudiness)
    {
        // Cloudiness should be 0-100%
        if (cloudiness < 0 || cloudiness > 100)
        {
            _logger.LogWarning("Invalid cloudiness value: {Cloudiness}. Clamping to valid range", cloudiness);
            return Math.Max(0, Math.Min(100, cloudiness));
        }
        return cloudiness;
    }

    private ComfortIndexDto CreateDefaultComfortIndex(int cityId, string cityName)
    {
        return new ComfortIndexDto
        {
            CityId = cityId,
            CityName = cityName,
            Score = 0,
            TemperaturePenalty = 0,
            HumidityPenalty = 0,
            WindPenalty = 0,
            CloudinessPenalty = 0
        };
    }
}

