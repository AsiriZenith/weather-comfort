using Microsoft.AspNetCore.Mvc;
using weather_comfort.Server.DTOs;
using weather_comfort.Server.Services;

namespace weather_comfort.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(
        IDashboardService dashboardService,
        ILogger<WeatherController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<IReadOnlyList<DashboardCityDto>>> GetDashboard(CancellationToken cancellationToken = default)
    {
        try
        {
            var dashboardData = await _dashboardService.GetDashboardDataAsync(cancellationToken);
            return Ok(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard data");
            return StatusCode(500, new { error = "An error occurred while fetching dashboard data" });
        }
    }
}

