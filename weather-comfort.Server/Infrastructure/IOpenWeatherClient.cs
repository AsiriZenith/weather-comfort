using weather_comfort.Server.Models;

namespace weather_comfort.Server.Infrastructure;

public interface IOpenWeatherClient
{
    Task<Weather> GetCurrentWeatherAsync(int cityId, CancellationToken cancellationToken = default);
}

