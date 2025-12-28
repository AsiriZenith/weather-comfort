using weather_comfort.Server.DTOs;

namespace weather_comfort.Server.Services;

public interface IDashboardService
{
    Task<IReadOnlyList<DashboardCityDto>> GetDashboardDataAsync(CancellationToken cancellationToken = default);
}

