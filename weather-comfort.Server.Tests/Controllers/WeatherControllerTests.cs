using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using weather_comfort.Server.Controllers;
using weather_comfort.Server.DTOs;
using weather_comfort.Server.Services;
using Xunit;

namespace weather_comfort.Server.Tests.Controllers;

public class WeatherControllerTests
{
    private readonly Mock<IWeatherService> _mockWeatherService;
    private readonly Mock<IComfortIndexService> _mockComfortIndexService;
    private readonly Mock<IRankingService> _mockRankingService;
    private readonly Mock<ILogger<WeatherController>> _mockLogger;
    private readonly WeatherController _controller;

    public WeatherControllerTests()
    {
        _mockWeatherService = new Mock<IWeatherService>();
        _mockComfortIndexService = new Mock<IComfortIndexService>();
        _mockRankingService = new Mock<IRankingService>();
        _mockLogger = new Mock<ILogger<WeatherController>>();
        _controller = new WeatherController(
            _mockWeatherService.Object,
            _mockComfortIndexService.Object,
            _mockRankingService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetDashboard_WithValidData_ReturnsOkWithDashboardCities()
    {
        // Arrange
        var weatherData = new List<WeatherDto>
        {
            new() 
            { 
                CityId = 2643743, 
                CityName = "London", 
                Temperature = 15.5, 
                Description = "clear sky" 
            },
            new() 
            { 
                CityId = 1850147, 
                CityName = "Tokyo", 
                Temperature = 22.3, 
                Description = "partly cloudy" 
            }
        }.AsReadOnly();

        var comfortIndices = new List<ComfortIndexDto>
        {
            new() { CityId = 2643743, CityName = "London", Score = 75.5 },
            new() { CityId = 1850147, CityName = "Tokyo", Score = 82.3 }
        }.AsReadOnly();

        var rankedCities = new List<RankedCityDto>
        {
            new() { Rank = 1, CityId = 1850147, CityName = "Tokyo", Score = 82.3 },
            new() { Rank = 2, CityId = 2643743, CityName = "London", Score = 75.5 }
        }.AsReadOnly();

        _mockWeatherService
            .Setup(x => x.GetWeatherForAllCitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(weatherData);

        _mockComfortIndexService
            .Setup(x => x.CalculateComfortIndexForAll(weatherData))
            .Returns(comfortIndices);

        _mockRankingService
            .Setup(x => x.RankCities(comfortIndices))
            .Returns(rankedCities);

        // Act
        var result = await _controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<ActionResult<IReadOnlyList<DashboardCityDto>>>(result);
        var actionResult = okResult.Result as OkObjectResult;
        Assert.NotNull(actionResult);

        var dashboardCities = Assert.IsAssignableFrom<IReadOnlyList<DashboardCityDto>>(actionResult.Value);
        Assert.NotNull(dashboardCities);
        Assert.Equal(2, dashboardCities.Count);

        var tokyo = dashboardCities.First(c => c.CityId == 1850147);
        Assert.Equal("Tokyo", tokyo.CityName);
        Assert.Equal(22.3, tokyo.Temperature);
        Assert.Equal("partly cloudy", tokyo.Description);
        Assert.Equal(82.3, tokyo.ComfortIndex);
        Assert.Equal(1, tokyo.Rank);

        var london = dashboardCities.First(c => c.CityId == 2643743);
        Assert.Equal("London", london.CityName);
        Assert.Equal(15.5, london.Temperature);
        Assert.Equal("clear sky", london.Description);
        Assert.Equal(75.5, london.ComfortIndex);
        Assert.Equal(2, london.Rank);
    }

    [Fact]
    public async Task GetDashboard_WithEmptyWeatherData_ReturnsOkWithEmptyList()
    {
        // Arrange
        var emptyWeatherData = new List<WeatherDto>().AsReadOnly();

        _mockWeatherService
            .Setup(x => x.GetWeatherForAllCitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyWeatherData);

        // Act
        var result = await _controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<ActionResult<IReadOnlyList<DashboardCityDto>>>(result);
        var actionResult = okResult.Result as OkObjectResult;
        Assert.NotNull(actionResult);

        var dashboardCities = Assert.IsAssignableFrom<IReadOnlyList<DashboardCityDto>>(actionResult.Value);
        Assert.NotNull(dashboardCities);
        Assert.Empty(dashboardCities);
    }

    [Fact]
    public async Task GetDashboard_WithNullWeatherData_ReturnsOkWithEmptyList()
    {
        // Arrange
        IReadOnlyList<WeatherDto>? nullWeatherData = null;

        _mockWeatherService
            .Setup(x => x.GetWeatherForAllCitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(nullWeatherData!);

        // Act
        var result = await _controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<ActionResult<IReadOnlyList<DashboardCityDto>>>(result);
        var actionResult = okResult.Result as OkObjectResult;
        Assert.NotNull(actionResult);

        var dashboardCities = Assert.IsAssignableFrom<IReadOnlyList<DashboardCityDto>>(actionResult.Value);
        Assert.NotNull(dashboardCities);
        Assert.Empty(dashboardCities);
    }

    [Fact]
    public async Task GetDashboard_WithWeatherServiceException_ReturnsInternalServerError()
    {
        // Arrange
        _mockWeatherService
            .Setup(x => x.GetWeatherForAllCitiesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Weather service error"));

        // Act
        var result = await _controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<ActionResult<IReadOnlyList<DashboardCityDto>>>(result);
        var statusCodeResult = okResult.Result as ObjectResult;
        Assert.NotNull(statusCodeResult);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_WithComfortIndexServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var weatherData = new List<WeatherDto>
        {
            new() { CityId = 2643743, CityName = "London", Temperature = 15.5, Description = "clear sky" }
        }.AsReadOnly();

        _mockWeatherService
            .Setup(x => x.GetWeatherForAllCitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(weatherData);

        _mockComfortIndexService
            .Setup(x => x.CalculateComfortIndexForAll(weatherData))
            .Throws(new Exception("Comfort index service error"));

        // Act
        var result = await _controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<ActionResult<IReadOnlyList<DashboardCityDto>>>(result);
        var statusCodeResult = okResult.Result as ObjectResult;
        Assert.NotNull(statusCodeResult);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_WithRankingServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var weatherData = new List<WeatherDto>
        {
            new() { CityId = 2643743, CityName = "London", Temperature = 15.5, Description = "clear sky" }
        }.AsReadOnly();

        var comfortIndices = new List<ComfortIndexDto>
        {
            new() { CityId = 2643743, CityName = "London", Score = 75.5 }
        }.AsReadOnly();

        _mockWeatherService
            .Setup(x => x.GetWeatherForAllCitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(weatherData);

        _mockComfortIndexService
            .Setup(x => x.CalculateComfortIndexForAll(weatherData))
            .Returns(comfortIndices);

        _mockRankingService
            .Setup(x => x.RankCities(comfortIndices))
            .Throws(new Exception("Ranking service error"));

        // Act
        var result = await _controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<ActionResult<IReadOnlyList<DashboardCityDto>>>(result);
        var statusCodeResult = okResult.Result as ObjectResult;
        Assert.NotNull(statusCodeResult);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_WithMissingWeatherData_HandlesGracefully()
    {
        // Arrange
        var weatherData = new List<WeatherDto>
        {
            new() { CityId = 2643743, CityName = "London", Temperature = 15.5, Description = "clear sky" }
        }.AsReadOnly();

        var comfortIndices = new List<ComfortIndexDto>
        {
            new() { CityId = 2643743, CityName = "London", Score = 75.5 },
            new() { CityId = 9999999, CityName = "Unknown", Score = 50.0 } // City not in weather data
        }.AsReadOnly();

        var rankedCities = new List<RankedCityDto>
        {
            new() { Rank = 1, CityId = 2643743, CityName = "London", Score = 75.5 },
            new() { Rank = 2, CityId = 9999999, CityName = "Unknown", Score = 50.0 }
        }.AsReadOnly();

        _mockWeatherService
            .Setup(x => x.GetWeatherForAllCitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(weatherData);

        _mockComfortIndexService
            .Setup(x => x.CalculateComfortIndexForAll(weatherData))
            .Returns(comfortIndices);

        _mockRankingService
            .Setup(x => x.RankCities(comfortIndices))
            .Returns(rankedCities);

        // Act
        var result = await _controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<ActionResult<IReadOnlyList<DashboardCityDto>>>(result);
        var actionResult = okResult.Result as OkObjectResult;
        Assert.NotNull(actionResult);

        var dashboardCities = Assert.IsAssignableFrom<IReadOnlyList<DashboardCityDto>>(actionResult.Value);
        Assert.NotNull(dashboardCities);
        Assert.Equal(2, dashboardCities.Count);

        var london = dashboardCities.First(c => c.CityId == 2643743);
        Assert.Equal("London", london.CityName);
        Assert.Equal(15.5, london.Temperature);
        Assert.Equal("clear sky", london.Description);

        var unknown = dashboardCities.First(c => c.CityId == 9999999);
        Assert.Equal("Unknown", unknown.CityName);
        Assert.Equal(0, unknown.Temperature); // Default when weather data not found
        Assert.Equal(string.Empty, unknown.Description); // Default when weather data not found
    }
}

