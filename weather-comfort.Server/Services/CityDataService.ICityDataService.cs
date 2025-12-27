using weather_comfort.Server.Models;

namespace weather_comfort.Server.Services;

public interface ICityDataService
{
    Task<IReadOnlyList<City>> GetCitiesAsync();
}
