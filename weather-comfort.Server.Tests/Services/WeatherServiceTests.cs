using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using weather_comfort.Server.DTOs;
using weather_comfort.Server.Infrastructure;
using weather_comfort.Server.Models;
using weather_comfort.Server.Services;
using Xunit;

namespace weather_comfort.Server.Tests.Services;

public class WeatherServiceTests
{
    private readonly Mock<ICityDataService> _mockCityDataService;
    private readonly Mock<IOpenWeatherClient> _mockOpenWeatherClient;
    private readonly Mock<ILogger<WeatherService>> _mockLogger;
    private readonly IMemoryCache _memoryCache;
    private readonly WeatherService _service;

    public WeatherServiceTests()
    {
        _mockCityDataService = new Mock<ICityDataService>();
        _mockOpenWeatherClient = new Mock<IOpenWeatherClient>();
        _mockLogger = new Mock<ILogger<WeatherService>>();
        _memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        _service = new WeatherService(_mockCityDataService.Object, _mockOpenWeatherClient.Object, _memoryCache, _mockLogger.Object);
    }

    [Fact]
    public async Task GetWeatherForCityAsync_WithValidData_ReturnsWeatherDtoWithCelsius()
    {
        // Arrange
        var cityId = 2643743;
        var cityName = "London";
        var weather = new Weather
        {
            Main = new MainData
            {
                Temp = 293.15, // 20°C in Kelvin
                FeelsLike = 292.15, // 19°C in Kelvin
                Humidity = 65
            },
            Wind = new WindData { Speed = 3.5 },
            Clouds = new CloudsData { All = 40 },
            WeatherDescriptions = new List<WeatherDescription>
            {
                new() { Description = "clear sky" }
            }
        };

        var cities = new List<weather_comfort.Server.Models.City>
        {
            new() { Id = cityId, Name = cityName, CountryCode = "GB" }
        }.AsReadOnly();

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather);

        _mockCityDataService
            .Setup(x => x.GetCitiesAsync())
            .ReturnsAsync(cities);

        // Act
        var result = await _service.GetWeatherForCityAsync(cityId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cityId, result.CityId);
        Assert.Equal(cityName, result.CityName);
        Assert.Equal(20.0, result.Temperature, 2); // 293.15 - 273.15 = 20.0
        Assert.Equal(19.0, result.FeelsLike, 2); // 292.15 - 273.15 = 19.0
        Assert.Equal(65, result.Humidity);
        Assert.Equal(3.5, result.WindSpeed);
        Assert.Equal(40, result.Cloudiness);
        Assert.Equal("clear sky", result.Description);
    }

    [Fact]
    public async Task GetWeatherForCityAsync_ConvertsKelvinToCelsius_Accurately()
    {
        // Arrange
        var cityId = 2643743;
        var testCases = new[]
        {
            new { Kelvin = 273.15, ExpectedCelsius = 0.0 }, // Freezing point
            new { Kelvin = 293.15, ExpectedCelsius = 20.0 }, // Room temperature
            new { Kelvin = 303.15, ExpectedCelsius = 30.0 }, // Hot day
            new { Kelvin = 263.15, ExpectedCelsius = -10.0 } // Cold day
        };

        var cities = new List<weather_comfort.Server.Models.City>
        {
            new() { Id = cityId, Name = "Test City", CountryCode = "XX" }
        }.AsReadOnly();

        _mockCityDataService
            .Setup(x => x.GetCitiesAsync())
            .ReturnsAsync(cities);

        foreach (var testCase in testCases)
        {
            // Clear cache before each test case to ensure fresh API call
            _memoryCache.Remove($"weather:{cityId}");
            
            var weather = new Weather
            {
                Main = new MainData
                {
                    Temp = testCase.Kelvin,
                    FeelsLike = testCase.Kelvin,
                    Humidity = 50
                },
                Wind = new WindData { Speed = 0 },
                Clouds = new CloudsData { All = 0 },
                WeatherDescriptions = new List<WeatherDescription>
                {
                    new() { Description = "test" }
                }
            };

            _mockOpenWeatherClient
                .Setup(x => x.GetCurrentWeatherAsync(cityId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(weather);

            // Act
            var result = await _service.GetWeatherForCityAsync(cityId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testCase.ExpectedCelsius, result.Temperature, 2);
            Assert.Equal(testCase.ExpectedCelsius, result.FeelsLike, 2);
        }
    }

    [Fact]
    public async Task GetWeatherForCityAsync_WithIncompleteData_ReturnsNull()
    {
        // Arrange
        var cityId = 2643743;
        var weather = new Weather
        {
            Main = null, // Missing main data
            WeatherDescriptions = null // Missing weather descriptions
        };

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather);

        _mockCityDataService
            .Setup(x => x.GetCitiesAsync())
            .ReturnsAsync(new List<weather_comfort.Server.Models.City>().AsReadOnly());

        // Act
        var result = await _service.GetWeatherForCityAsync(cityId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetWeatherForCityAsync_WithEmptyWeatherDescriptions_ReturnsNull()
    {
        // Arrange
        var cityId = 2643743;
        var weather = new Weather
        {
            Main = new MainData { Temp = 293.15, FeelsLike = 293.15, Humidity = 50 },
            WeatherDescriptions = new List<WeatherDescription>() // Empty list
        };

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather);

        _mockCityDataService
            .Setup(x => x.GetCitiesAsync())
            .ReturnsAsync(new List<weather_comfort.Server.Models.City>().AsReadOnly());

        // Act
        var result = await _service.GetWeatherForCityAsync(cityId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetWeatherForAllCitiesAsync_ReturnsWeatherForAllCities()
    {
        // Arrange
        var cities = new List<weather_comfort.Server.Models.City>
        {
            new() { Id = 2643743, Name = "London", CountryCode = "GB" },
            new() { Id = 1850147, Name = "Tokyo", CountryCode = "JP" }
        }.AsReadOnly();

        var weather1 = new Weather
        {
            Main = new MainData { Temp = 293.15, FeelsLike = 292.15, Humidity = 65 },
            Wind = new WindData { Speed = 3.5 },
            Clouds = new CloudsData { All = 40 },
            WeatherDescriptions = new List<WeatherDescription> { new() { Description = "clear sky" } }
        };

        var weather2 = new Weather
        {
            Main = new MainData { Temp = 298.15, FeelsLike = 297.15, Humidity = 70 },
            Wind = new WindData { Speed = 2.0 },
            Clouds = new CloudsData { All = 20 },
            WeatherDescriptions = new List<WeatherDescription> { new() { Description = "few clouds" } }
        };

        _mockCityDataService
            .Setup(x => x.GetCitiesAsync())
            .ReturnsAsync(cities);

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(2643743, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather1);

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(1850147, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather2);

        // Act
        var result = await _service.GetWeatherForAllCitiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, w => w.CityId == 2643743 && w.CityName == "London");
        Assert.Contains(result, w => w.CityId == 1850147 && w.CityName == "Tokyo");
    }

    [Fact]
    public async Task GetWeatherForAllCitiesAsync_WithPartialFailures_ContinuesAndReturnsSuccessfulResults()
    {
        // Arrange
        var cities = new List<weather_comfort.Server.Models.City>
        {
            new() { Id = 2643743, Name = "London", CountryCode = "GB" },
            new() { Id = 999999, Name = "Invalid City", CountryCode = "XX" },
            new() { Id = 1850147, Name = "Tokyo", CountryCode = "JP" }
        }.AsReadOnly();

        var weather1 = new Weather
        {
            Main = new MainData { Temp = 293.15, FeelsLike = 292.15, Humidity = 65 },
            Wind = new WindData { Speed = 3.5 },
            Clouds = new CloudsData { All = 40 },
            WeatherDescriptions = new List<WeatherDescription> { new() { Description = "clear sky" } }
        };

        var weather2 = new Weather
        {
            Main = new MainData { Temp = 298.15, FeelsLike = 297.15, Humidity = 70 },
            Wind = new WindData { Speed = 2.0 },
            Clouds = new CloudsData { All = 20 },
            WeatherDescriptions = new List<WeatherDescription> { new() { Description = "few clouds" } }
        };

        _mockCityDataService
            .Setup(x => x.GetCitiesAsync())
            .ReturnsAsync(cities);

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(2643743, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather1);

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(999999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("City not found"));

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(1850147, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather2);

        // Act
        var result = await _service.GetWeatherForAllCitiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Should have 2 successful results, skipping the failed one
        Assert.Contains(result, w => w.CityId == 2643743);
        Assert.Contains(result, w => w.CityId == 1850147);
        Assert.DoesNotContain(result, w => w.CityId == 999999);
    }

    [Fact]
    public async Task GetWeatherForCityAsync_WithMissingWindData_UsesDefaultValue()
    {
        // Arrange
        var cityId = 2643743;
        var weather = new Weather
        {
            Main = new MainData { Temp = 293.15, FeelsLike = 292.15, Humidity = 65 },
            Wind = null, // Missing wind data
            Clouds = new CloudsData { All = 40 },
            WeatherDescriptions = new List<WeatherDescription> { new() { Description = "clear sky" } }
        };

        var cities = new List<weather_comfort.Server.Models.City>
        {
            new() { Id = cityId, Name = "Test City", CountryCode = "XX" }
        }.AsReadOnly();

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather);

        _mockCityDataService
            .Setup(x => x.GetCitiesAsync())
            .ReturnsAsync(cities);

        // Act
        var result = await _service.GetWeatherForCityAsync(cityId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.WindSpeed); // Should default to 0
    }

    [Fact]
    public async Task GetWeatherForCityAsync_WithMissingCloudsData_UsesDefaultValue()
    {
        // Arrange
        var cityId = 2643743;
        var weather = new Weather
        {
            Main = new MainData { Temp = 293.15, FeelsLike = 292.15, Humidity = 65 },
            Wind = new WindData { Speed = 3.5 },
            Clouds = null, // Missing clouds data
            WeatherDescriptions = new List<WeatherDescription> { new() { Description = "clear sky" } }
        };

        var cities = new List<weather_comfort.Server.Models.City>
        {
            new() { Id = cityId, Name = "Test City", CountryCode = "XX" }
        }.AsReadOnly();

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather);

        _mockCityDataService
            .Setup(x => x.GetCitiesAsync())
            .ReturnsAsync(cities);

        // Act
        var result = await _service.GetWeatherForCityAsync(cityId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Cloudiness); // Should default to 0
    }
}

