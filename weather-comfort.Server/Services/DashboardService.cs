using weather_comfort.Server.DTOs;

namespace weather_comfort.Server.Services;

public class DashboardService : IDashboardService
{
    private readonly IWeatherService _weatherService;
    private readonly IComfortIndexService _comfortIndexService;
    private readonly IRankingService _rankingService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IWeatherService weatherService,
        IComfortIndexService comfortIndexService,
        IRankingService rankingService,
        ILogger<DashboardService> logger)
    {
        _weatherService = weatherService;
        _comfortIndexService = comfortIndexService;
        _rankingService = rankingService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DashboardCityDto>> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching dashboard data for all cities");

        // Step 1: Get weather data for all cities
        var weatherData = await _weatherService.GetWeatherForAllCitiesAsync(cancellationToken);
        
        if (weatherData == null || weatherData.Count == 0)
        {
            _logger.LogInformation("No weather data available");
            return new List<DashboardCityDto>().AsReadOnly();
        }

        // Step 2: Calculate comfort indices
        var comfortIndices = _comfortIndexService.CalculateComfortIndexForAll(weatherData);
        
        if (comfortIndices == null || comfortIndices.Count == 0)
        {
            _logger.LogWarning("No comfort indices calculated from weather data. Weather data count: {Count}", weatherData.Count);
            return new List<DashboardCityDto>().AsReadOnly();
        }

        // Step 3: Rank cities by comfort index
        var rankedCities = _rankingService.RankCities(comfortIndices);
        
        if (rankedCities == null || rankedCities.Count == 0)
        {
            _logger.LogWarning("No ranked cities returned from ranking service. Comfort indices count: {Count}", comfortIndices.Count);
            return new List<DashboardCityDto>().AsReadOnly();
        }

        // Step 4: Combine data from WeatherDto and RankedCityDto into DashboardCityDto
        var dashboardData = rankedCities
            .Select(rankedCity =>
            {
                var weather = weatherData.FirstOrDefault(w => w.CityId == rankedCity.CityId);
                
                return new DashboardCityDto
                {
                    CityId = rankedCity.CityId,
                    CityName = rankedCity.CityName ?? string.Empty,
                    Description = weather?.Description ?? string.Empty,
                    Temperature = weather?.Temperature ?? 0,
                    ComfortIndex = rankedCity.Score,
                    Rank = rankedCity.Rank
                };
            })
            .ToList();

        if (dashboardData == null || dashboardData.Count == 0)
        {
            _logger.LogWarning("Dashboard data list is null or empty after combining data");
            return new List<DashboardCityDto>().AsReadOnly();
        }

        _logger.LogInformation("Successfully prepared dashboard data for {Count} cities", dashboardData.Count);

        return dashboardData.AsReadOnly();
    }
}

