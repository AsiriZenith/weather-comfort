using weather_comfort.Server.DTOs;

namespace weather_comfort.Server.Services;

public interface IWeatherService
{
    Task<IReadOnlyList<WeatherDto>> GetWeatherForAllCitiesAsync(CancellationToken cancellationToken = default);

    Task<WeatherDto?> GetWeatherForCityAsync(int cityId, CancellationToken cancellationToken = default);
}

