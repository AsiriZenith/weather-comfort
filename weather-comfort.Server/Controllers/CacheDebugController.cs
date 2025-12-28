using Microsoft.AspNetCore.Mvc;
using weather_comfort.Server.DTOs;
using weather_comfort.Server.Services;

namespace weather_comfort.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CacheDebugController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<CacheDebugController> _logger;

    public CacheDebugController(
        IWeatherService weatherService,
        ILogger<CacheDebugController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    [HttpGet("{cityId}")]
    public ActionResult<CacheStatusDto> GetCacheStatus(int cityId)
    {
        try
        {
            var status = _weatherService.GetCacheStatus(cityId);
            
            var result = new CacheStatusDto
            {
                CityId = cityId,
                Status = status
            };

            _logger.LogInformation("Cache status check for city ID {CityId}: {Status}", cityId, status);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache status for city ID {CityId}", cityId);
            return StatusCode(500, new { error = "An error occurred while checking cache status" });
        }
    }

    [HttpGet("all")]
    public async Task<ActionResult<IReadOnlyList<CacheStatusDto>>> GetAllCacheStatuses(CancellationToken cancellationToken = default)
    {
        try
        {
            var weatherData = await _weatherService.GetWeatherForAllCitiesAsync(cancellationToken);
            var cityIds = weatherData.Select(w => w.CityId).Distinct().ToList();
            
            var results = cityIds.Select(cityId => new CacheStatusDto
            {
                CityId = cityId,
                Status = _weatherService.GetCacheStatus(cityId)
            }).ToList().AsReadOnly();

            _logger.LogInformation("Cache status check for {Count} cities", results.Count);
            
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache statuses for all cities");
            return StatusCode(500, new { error = "An error occurred while checking cache statuses" });
        }
    }
}

