using weather_comfort.Server.DTOs;
using weather_comfort.Server.Infrastructure;

namespace weather_comfort.Server.Services;

public class WeatherService : IWeatherService
{
    private const double KelvinToCelsiusOffset = 273.15;
    private readonly ICityDataService _cityDataService;
    private readonly IOpenWeatherClient _openWeatherClient;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        ICityDataService cityDataService,
        IOpenWeatherClient openWeatherClient,
        ILogger<WeatherService> logger)
    {
        _cityDataService = cityDataService;
        _openWeatherClient = openWeatherClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WeatherDto>> GetWeatherForAllCitiesAsync(CancellationToken cancellationToken = default)
    {
        var cities = await _cityDataService.GetCitiesAsync();
        var weatherDtos = new List<WeatherDto>();

        _logger.LogInformation("Retrieving weather data for {Count} cities", cities.Count);

        foreach (var city in cities)
        {
            try
            {
                var weatherDto = await GetWeatherForCityAsync(city.Id, cancellationToken);
                if (weatherDto != null)
                {
                    weatherDtos.Add(weatherDto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve weather data for city ID {CityId} ({CityName}). Skipping.", 
                    city.Id, city.Name);
            }
        }

        _logger.LogInformation("Successfully retrieved weather data for {Count} out of {Total} cities", 
            weatherDtos.Count, cities.Count);

        return weatherDtos.AsReadOnly();
    }

    public async Task<WeatherDto?> GetWeatherForCityAsync(int cityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var weather = await _openWeatherClient.GetCurrentWeatherAsync(cityId, cancellationToken);

            if (weather?.Main == null || weather.WeatherDescriptions == null || weather.WeatherDescriptions.Count == 0)
            {
                _logger.LogWarning("Incomplete weather data received for city ID {CityId}", cityId);
                return null;
            }

            var cities = await _cityDataService.GetCitiesAsync();
            var city = cities.FirstOrDefault(c => c.Id == cityId);
            var cityName = city?.Name ?? "Unknown";

            var weatherDto = new WeatherDto
            {
                CityId = cityId,
                CityName = cityName,
                Temperature = ConvertKelvinToCelsius(weather.Main.Temp),
                FeelsLike = ConvertKelvinToCelsius(weather.Main.FeelsLike),
                Humidity = weather.Main.Humidity,
                WindSpeed = weather.Wind?.Speed ?? 0,
                Cloudiness = weather.Clouds?.All ?? 0,
                Description = weather.WeatherDescriptions[0].Description
            };

            return weatherDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather data for city ID {CityId}", cityId);
            throw;
        }
    }

    private static double ConvertKelvinToCelsius(double kelvin)
    {
        return kelvin - KelvinToCelsiusOffset;
    }
}

