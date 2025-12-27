using weather_comfort.Server.DTOs;

namespace weather_comfort.Server.Services;

public interface IComfortIndexService
{
    ComfortIndexDto CalculateComfortIndex(WeatherDto weather);
    
    IReadOnlyList<ComfortIndexDto> CalculateComfortIndexForAll(IReadOnlyList<WeatherDto> weatherList);
}

