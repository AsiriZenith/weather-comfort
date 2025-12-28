using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using weather_comfort.Server.Controllers;
using weather_comfort.Server.DTOs;
using weather_comfort.Server.Services;
using Xunit;

namespace weather_comfort.Server.Tests.Controllers;

public class CacheDebugControllerTests
{
    private readonly Mock<IWeatherService> _mockWeatherService;
    private readonly Mock<ILogger<CacheDebugController>> _mockLogger;
    private readonly CacheDebugController _controller;

    public CacheDebugControllerTests()
    {
        _mockWeatherService = new Mock<IWeatherService>();
        _mockLogger = new Mock<ILogger<CacheDebugController>>();
        _controller = new CacheDebugController(_mockWeatherService.Object, _mockLogger.Object);
    }

    [Fact]
    public void GetCacheStatus_WithValidCityId_ReturnsOkWithHitStatus()
    {
        // Arrange
        var cityId = 2643743;
        _mockWeatherService
            .Setup(x => x.GetCacheStatus(cityId))
            .Returns("HIT");

        // Act
        var result = _controller.GetCacheStatus(cityId);

        // Assert
        var okResult = Assert.IsType<ActionResult<CacheStatusDto>>(result);
        var actionResult = okResult.Result as OkObjectResult;
        Assert.NotNull(actionResult);
        
        var cacheStatus = Assert.IsType<CacheStatusDto>(actionResult.Value);
        Assert.Equal(cityId, cacheStatus.CityId);
        Assert.Equal("HIT", cacheStatus.Status);
    }

    [Fact]
    public void GetCacheStatus_WithValidCityId_ReturnsOkWithMissStatus()
    {
        // Arrange
        var cityId = 2643743;
        _mockWeatherService
            .Setup(x => x.GetCacheStatus(cityId))
            .Returns("MISS");

        // Act
        var result = _controller.GetCacheStatus(cityId);

        // Assert
        var okResult = Assert.IsType<ActionResult<CacheStatusDto>>(result);
        var actionResult = okResult.Result as OkObjectResult;
        Assert.NotNull(actionResult);
        
        var cacheStatus = Assert.IsType<CacheStatusDto>(actionResult.Value);
        Assert.Equal(cityId, cacheStatus.CityId);
        Assert.Equal("MISS", cacheStatus.Status);
    }

    [Fact]
    public void GetCacheStatus_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var cityId = 2643743;
        _mockWeatherService
            .Setup(x => x.GetCacheStatus(cityId))
            .Throws(new Exception("Service error"));

        // Act
        var result = _controller.GetCacheStatus(cityId);

        // Assert
        var okResult = Assert.IsType<ActionResult<CacheStatusDto>>(result);
        var statusCodeResult = okResult.Result as ObjectResult;
        Assert.NotNull(statusCodeResult);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetAllCacheStatuses_WithValidData_ReturnsOkWithList()
    {
        // Arrange
        var weatherDtos = new List<WeatherDto>
        {
            new() { CityId = 2643743, CityName = "London" },
            new() { CityId = 1850147, CityName = "Tokyo" }
        }.AsReadOnly();

        _mockWeatherService
            .Setup(x => x.GetWeatherForAllCitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(weatherDtos);

        _mockWeatherService
            .Setup(x => x.GetCacheStatus(2643743))
            .Returns("HIT");

        _mockWeatherService
            .Setup(x => x.GetCacheStatus(1850147))
            .Returns("MISS");

        // Act
        var result = await _controller.GetAllCacheStatuses();

        // Assert
        var okResult = Assert.IsType<ActionResult<IReadOnlyList<CacheStatusDto>>>(result);
        var actionResult = okResult.Result as OkObjectResult;
        Assert.NotNull(actionResult);
        
        var cacheStatuses = Assert.IsAssignableFrom<IReadOnlyList<CacheStatusDto>>(actionResult.Value);
        Assert.NotNull(cacheStatuses);
        Assert.Equal(2, cacheStatuses.Count);
        Assert.Contains(cacheStatuses, cs => cs.CityId == 2643743 && cs.Status == "HIT");
        Assert.Contains(cacheStatuses, cs => cs.CityId == 1850147 && cs.Status == "MISS");
    }

    [Fact]
    public async Task GetAllCacheStatuses_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        _mockWeatherService
            .Setup(x => x.GetWeatherForAllCitiesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetAllCacheStatuses();

        // Assert
        var okResult = Assert.IsType<ActionResult<IReadOnlyList<CacheStatusDto>>>(result);
        var statusCodeResult = okResult.Result as ObjectResult;
        Assert.NotNull(statusCodeResult);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetAllCacheStatuses_WithEmptyList_ReturnsOkWithEmptyList()
    {
        // Arrange
        var emptyList = new List<WeatherDto>().AsReadOnly();

        _mockWeatherService
            .Setup(x => x.GetWeatherForAllCitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _controller.GetAllCacheStatuses();

        // Assert
        var okResult = Assert.IsType<ActionResult<IReadOnlyList<CacheStatusDto>>>(result);
        var actionResult = okResult.Result as OkObjectResult;
        Assert.NotNull(actionResult);
        
        var cacheStatuses = Assert.IsAssignableFrom<IReadOnlyList<CacheStatusDto>>(actionResult.Value);
        Assert.NotNull(cacheStatuses);
        Assert.Empty(cacheStatuses);
    }
}

