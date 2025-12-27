using Microsoft.Extensions.Logging;
using Moq;
using weather_comfort.Server.DTOs;
using weather_comfort.Server.Services;
using Xunit;

namespace weather_comfort.Server.Tests.Services;

public class ComfortIndexServiceTests
{
    private readonly Mock<ILogger<ComfortIndexService>> _mockLogger;
    private readonly ComfortIndexService _service;

    public ComfortIndexServiceTests()
    {
        _mockLogger = new Mock<ILogger<ComfortIndexService>>();
        _service = new ComfortIndexService(_mockLogger.Object);
    }

    [Fact]
    public void CalculateComfortIndex_WithIdealConditions_ReturnsHighScore()
    {
        // Arrange - Ideal conditions: 22°C, 50% humidity, 5 m/s wind, 30% cloudiness
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Test City",
            Temperature = 22.0,
            Humidity = 50,
            WindSpeed = 5.0,
            Cloudiness = 30
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.CityId);
        Assert.Equal("Test City", result.CityName);
        Assert.True(result.Score >= 95, $"Expected score >= 95, got {result.Score}");
        Assert.True(result.Score <= 100, $"Expected score <= 100, got {result.Score}");
        Assert.Equal(0, result.TemperaturePenalty);
        Assert.Equal(0, result.HumidityPenalty);
        Assert.Equal(0, result.WindPenalty);
        Assert.Equal(0, result.CloudinessPenalty);
    }

    [Fact]
    public void CalculateComfortIndex_WithExtremeHotTemperature_AppliesSignificantPenalty()
    {
        // Arrange - Very hot: 40°C
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Hot City",
            Temperature = 40.0,
            Humidity = 50,
            WindSpeed = 5.0,
            Cloudiness = 30
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Score < 80, $"Expected score < 80 for hot temperature, got {result.Score}");
        Assert.True(result.TemperaturePenalty > 0);
        // Temperature penalty: (40 - 22) * 1.5 = 27 points
        Assert.True(result.TemperaturePenalty >= 25);
    }

    [Fact]
    public void CalculateComfortIndex_WithExtremeColdTemperature_AppliesSignificantPenalty()
    {
        // Arrange - Very cold: -10°C
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Cold City",
            Temperature = -10.0,
            Humidity = 50,
            WindSpeed = 5.0,
            Cloudiness = 30
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Score < 80, $"Expected score < 80 for cold temperature, got {result.Score}");
        Assert.True(result.TemperaturePenalty > 0);
        // Temperature penalty: |-10 - 22| * 1.5 = 48 points
        Assert.True(result.TemperaturePenalty >= 45);
    }

    [Fact]
    public void CalculateComfortIndex_WithHighHumidity_AppliesPenalty()
    {
        // Arrange - High humidity: 90%
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Humid City",
            Temperature = 22.0,
            Humidity = 90,
            WindSpeed = 5.0,
            Cloudiness = 30
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.HumidityPenalty > 0);
        // Humidity penalty: (90 - 60) * 0.5 = 15 points
        Assert.True(result.HumidityPenalty >= 14);
        Assert.True(result.Score < 100);
    }

    [Fact]
    public void CalculateComfortIndex_WithLowHumidity_AppliesPenalty()
    {
        // Arrange - Low humidity: 20%
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Dry City",
            Temperature = 22.0,
            Humidity = 20,
            WindSpeed = 5.0,
            Cloudiness = 30
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.HumidityPenalty > 0);
        // Humidity penalty: (40 - 20) * 0.5 = 10 points
        Assert.True(result.HumidityPenalty >= 9);
        Assert.True(result.Score < 100);
    }

    [Fact]
    public void CalculateComfortIndex_WithHighWindSpeed_AppliesPenalty()
    {
        // Arrange - High wind: 15 m/s
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Windy City",
            Temperature = 22.0,
            Humidity = 50,
            WindSpeed = 15.0,
            Cloudiness = 30
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.WindPenalty > 0);
        // Wind penalty: (15 - 5) * 2.0 = 20 points
        Assert.True(result.WindPenalty >= 19);
        Assert.True(result.Score < 100);
    }

    [Fact]
    public void CalculateComfortIndex_WithHighCloudiness_AppliesPenalty()
    {
        // Arrange - High cloudiness: 100%
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Cloudy City",
            Temperature = 22.0,
            Humidity = 50,
            WindSpeed = 5.0,
            Cloudiness = 100
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.CloudinessPenalty > 0);
        // Cloudiness penalty: (100 - 30) * 0.3 = 21 points
        Assert.True(result.CloudinessPenalty >= 20);
        Assert.True(result.Score < 100);
    }

    [Fact]
    public void CalculateComfortIndex_WithNullWeather_HandlesGracefully()
    {
        // Act
        var result = _service.CalculateComfortIndex(null!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Score);
        Assert.Equal(0, result.CityId);
        Assert.Equal("Unknown", result.CityName);
    }

    [Fact]
    public void CalculateComfortIndex_WithInvalidHumidity_ClampsToValidRange()
    {
        // Arrange - Invalid humidity: 150%
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Test City",
            Temperature = 22.0,
            Humidity = 150, // Invalid
            WindSpeed = 5.0,
            Cloudiness = 30
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.NotNull(result);
        // Should clamp to 100 and apply penalty
        Assert.True(result.Score >= 0 && result.Score <= 100);
    }

    [Fact]
    public void CalculateComfortIndex_WithNegativeHumidity_ClampsToValidRange()
    {
        // Arrange - Invalid humidity: -10%
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Test City",
            Temperature = 22.0,
            Humidity = -10, // Invalid
            WindSpeed = 5.0,
            Cloudiness = 30
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.NotNull(result);
        // Should clamp to 0 and apply penalty
        Assert.True(result.Score >= 0 && result.Score <= 100);
    }

    [Fact]
    public void CalculateComfortIndex_WithInvalidCloudiness_ClampsToValidRange()
    {
        // Arrange - Invalid cloudiness: 150%
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Test City",
            Temperature = 22.0,
            Humidity = 50,
            WindSpeed = 5.0,
            Cloudiness = 150 // Invalid
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.NotNull(result);
        // Should clamp to 100 and apply penalty
        Assert.True(result.Score >= 0 && result.Score <= 100);
    }

    [Fact]
    public void CalculateComfortIndex_WithNegativeWindSpeed_UsesZero()
    {
        // Arrange - Invalid wind speed: -5 m/s
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Test City",
            Temperature = 22.0,
            Humidity = 50,
            WindSpeed = -5.0, // Invalid
            Cloudiness = 30
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.NotNull(result);
        // Should use 0 and no penalty
        Assert.Equal(0, result.WindPenalty);
        Assert.True(result.Score >= 0 && result.Score <= 100);
    }

    [Fact]
    public void CalculateComfortIndex_ScoreNeverExceeds100()
    {
        // Arrange - Perfect conditions
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Perfect City",
            Temperature = 22.0,
            Humidity = 50,
            WindSpeed = 5.0,
            Cloudiness = 30
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.True(result.Score <= 100, $"Score should not exceed 100, got {result.Score}");
    }

    [Fact]
    public void CalculateComfortIndex_ScoreNeverGoesBelow0()
    {
        // Arrange - Extreme conditions
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Extreme City",
            Temperature = 50.0, // Very hot
            Humidity = 100, // Maximum
            WindSpeed = 30.0, // Very windy
            Cloudiness = 100 // Maximum
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.True(result.Score >= 0, $"Score should not go below 0, got {result.Score}");
    }

    [Fact]
    public void CalculateComfortIndex_UsesAllRequiredParameters()
    {
        // Arrange
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Test City",
            Temperature = 25.0, // Deviation from ideal
            Humidity = 70, // Deviation from ideal
            WindSpeed = 10.0, // Deviation from ideal
            Cloudiness = 50 // Deviation from ideal
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.NotNull(result);
        // All parameters should contribute to penalties
        Assert.True(result.TemperaturePenalty > 0, "Temperature should contribute to penalty");
        Assert.True(result.HumidityPenalty > 0, "Humidity should contribute to penalty");
        Assert.True(result.WindPenalty > 0, "Wind speed should contribute to penalty");
        Assert.True(result.CloudinessPenalty > 0, "Cloudiness should contribute to penalty");
    }

    [Fact]
    public void CalculateComfortIndexForAll_WithValidList_ReturnsResultsForAll()
    {
        // Arrange
        var weatherList = new List<WeatherDto>
        {
            new() { CityId = 1, CityName = "City 1", Temperature = 22.0, Humidity = 50, WindSpeed = 5.0, Cloudiness = 30 },
            new() { CityId = 2, CityName = "City 2", Temperature = 25.0, Humidity = 60, WindSpeed = 8.0, Cloudiness = 40 },
            new() { CityId = 3, CityName = "City 3", Temperature = 18.0, Humidity = 45, WindSpeed = 3.0, Cloudiness = 20 }
        };

        // Act
        var results = _service.CalculateComfortIndexForAll(weatherList);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.True(r.Score >= 0 && r.Score <= 100));
    }

    [Fact]
    public void CalculateComfortIndexForAll_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var weatherList = new List<WeatherDto>();

        // Act
        var results = _service.CalculateComfortIndexForAll(weatherList);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public void CalculateComfortIndexForAll_WithNullList_ReturnsEmptyList()
    {
        // Act
        var results = _service.CalculateComfortIndexForAll(null!);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public void CalculateComfortIndex_WithExtremeConditions_StillReturnsValidScore()
    {
        // Arrange - All extreme conditions
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Extreme City",
            Temperature = -20.0, // Very cold
            Humidity = 0, // Very dry
            WindSpeed = 50.0, // Very windy
            Cloudiness = 100 // Maximum cloudiness
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Score >= 0 && result.Score <= 100);
        // Score should be very low but not negative
        Assert.True(result.Score < 50, $"Expected very low score for extreme conditions, got {result.Score}");
    }

    [Fact]
    public void CalculateComfortIndex_WithNaNValues_HandlesGracefully()
    {
        // Arrange - Invalid temperature
        var weather = new WeatherDto
        {
            CityId = 1,
            CityName = "Test City",
            Temperature = double.NaN,
            Humidity = 50,
            WindSpeed = 5.0,
            Cloudiness = 30
        };

        // Act
        var result = _service.CalculateComfortIndex(weather);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Score >= 0 && result.Score <= 100);
    }
}

