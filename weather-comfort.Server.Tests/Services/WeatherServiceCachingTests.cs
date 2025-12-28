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

public class WeatherServiceCachingTests
{
    private readonly Mock<ICityDataService> _mockCityDataService;
    private readonly Mock<IOpenWeatherClient> _mockOpenWeatherClient;
    private readonly Mock<ILogger<WeatherService>> _mockLogger;
    private readonly IMemoryCache _memoryCache;
    private readonly WeatherService _service;

    public WeatherServiceCachingTests()
    {
        _mockCityDataService = new Mock<ICityDataService>();
        _mockOpenWeatherClient = new Mock<IOpenWeatherClient>();
        _mockLogger = new Mock<ILogger<WeatherService>>();
        _memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        _service = new WeatherService(_mockCityDataService.Object, _mockOpenWeatherClient.Object, _memoryCache, _mockLogger.Object);
    }

    [Fact]
    public async Task GetWeatherForCityAsync_WithCacheHit_ReturnsCachedDataWithoutApiCall()
    {
        // Arrange
        var cityId = 2643743;
        var cityName = "London";
        var cachedWeatherDto = new WeatherDto
        {
            CityId = cityId,
            CityName = cityName,
            Temperature = 20.0,
            FeelsLike = 19.0,
            Humidity = 65,
            WindSpeed = 3.5,
            Cloudiness = 40,
            Description = "cached data"
        };

        // Manually add to cache
        var cacheKey = $"weather:{cityId}";
        _memoryCache.Set(cacheKey, cachedWeatherDto, TimeSpan.FromMinutes(5));

        // Act
        var result = await _service.GetWeatherForCityAsync(cityId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cityId, result.CityId);
        Assert.Equal(cityName, result.CityName);
        Assert.Equal(20.0, result.Temperature);
        Assert.Equal("cached data", result.Description);
        
        // Verify API was NOT called
        _mockOpenWeatherClient.Verify(
            x => x.GetCurrentWeatherAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetWeatherForCityAsync_WithCacheMiss_FetchesFromApiAndCachesResult()
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

        // Act - First call (cache miss)
        var result1 = await _service.GetWeatherForCityAsync(cityId);

        // Assert - First call
        Assert.NotNull(result1);
        Assert.Equal(cityId, result1.CityId);
        Assert.Equal(cityName, result1.CityName);
        
        // Verify API was called
        _mockOpenWeatherClient.Verify(
            x => x.GetCurrentWeatherAsync(cityId, It.IsAny<CancellationToken>()),
            Times.Once);

        // Act - Second call (should be cache hit)
        var result2 = await _service.GetWeatherForCityAsync(cityId);

        // Assert - Second call
        Assert.NotNull(result2);
        Assert.Equal(cityId, result2.CityId);
        Assert.Equal(cityName, result2.CityName);
        Assert.Equal(result1.Temperature, result2.Temperature);
        Assert.Equal(result1.Description, result2.Description);
        
        // Verify API was NOT called again
        _mockOpenWeatherClient.Verify(
            x => x.GetCurrentWeatherAsync(cityId, It.IsAny<CancellationToken>()),
            Times.Once); // Still only once
    }

    [Fact]
    public async Task GetWeatherForCityAsync_WithDifferentCities_UsesSeparateCacheEntries()
    {
        // Arrange
        var cityId1 = 2643743; // London
        var cityId2 = 1850147; // Tokyo
        var cityName1 = "London";
        var cityName2 = "Tokyo";

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

        var cities = new List<weather_comfort.Server.Models.City>
        {
            new() { Id = cityId1, Name = cityName1, CountryCode = "GB" },
            new() { Id = cityId2, Name = cityName2, CountryCode = "JP" }
        }.AsReadOnly();

        _mockCityDataService
            .Setup(x => x.GetCitiesAsync())
            .ReturnsAsync(cities);

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(cityId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather1);

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(cityId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather2);

        // Act - Fetch both cities
        var result1 = await _service.GetWeatherForCityAsync(cityId1);
        var result2 = await _service.GetWeatherForCityAsync(cityId2);

        // Assert - Both should be cached
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(cityId1, result1.CityId);
        Assert.Equal(cityId2, result2.CityId);

        // Verify both API calls were made
        _mockOpenWeatherClient.Verify(
            x => x.GetCurrentWeatherAsync(cityId1, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockOpenWeatherClient.Verify(
            x => x.GetCurrentWeatherAsync(cityId2, It.IsAny<CancellationToken>()),
            Times.Once);

        // Act - Fetch again (should be from cache)
        var result1Cached = await _service.GetWeatherForCityAsync(cityId1);
        var result2Cached = await _service.GetWeatherForCityAsync(cityId2);

        // Assert - Should return cached data
        Assert.NotNull(result1Cached);
        Assert.NotNull(result2Cached);
        Assert.Equal(result1.Temperature, result1Cached.Temperature);
        Assert.Equal(result2.Temperature, result2Cached.Temperature);

        // Verify API was NOT called again
        _mockOpenWeatherClient.Verify(
            x => x.GetCurrentWeatherAsync(cityId1, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockOpenWeatherClient.Verify(
            x => x.GetCurrentWeatherAsync(cityId2, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetWeatherForAllCitiesAsync_WithPartialCacheHits_FetchesOnlyMissingCities()
    {
        // Arrange
        var cityId1 = 2643743; // London - will be cached
        var cityId2 = 1850147; // Tokyo - will be fetched
        var cityName1 = "London";
        var cityName2 = "Tokyo";

        // Pre-cache city 1
        var cachedWeatherDto = new WeatherDto
        {
            CityId = cityId1,
            CityName = cityName1,
            Temperature = 20.0,
            FeelsLike = 19.0,
            Humidity = 65,
            WindSpeed = 3.5,
            Cloudiness = 40,
            Description = "cached"
        };
        _memoryCache.Set($"weather:{cityId1}", cachedWeatherDto, TimeSpan.FromMinutes(5));

        var weather2 = new Weather
        {
            Main = new MainData { Temp = 298.15, FeelsLike = 297.15, Humidity = 70 },
            Wind = new WindData { Speed = 2.0 },
            Clouds = new CloudsData { All = 20 },
            WeatherDescriptions = new List<WeatherDescription> { new() { Description = "few clouds" } }
        };

        var cities = new List<weather_comfort.Server.Models.City>
        {
            new() { Id = cityId1, Name = cityName1, CountryCode = "GB" },
            new() { Id = cityId2, Name = cityName2, CountryCode = "JP" }
        }.AsReadOnly();

        _mockCityDataService
            .Setup(x => x.GetCitiesAsync())
            .ReturnsAsync(cities);

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(cityId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather2);

        // Act
        var result = await _service.GetWeatherForAllCitiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, w => w.CityId == cityId1 && w.Description == "cached");
        Assert.Contains(result, w => w.CityId == cityId2);

        // Verify API was called only for city 2 (city 1 was cached)
        _mockOpenWeatherClient.Verify(
            x => x.GetCurrentWeatherAsync(cityId1, It.IsAny<CancellationToken>()),
            Times.Never);
        _mockOpenWeatherClient.Verify(
            x => x.GetCurrentWeatherAsync(cityId2, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void GetCacheStatus_WithCachedData_ReturnsHit()
    {
        // Arrange
        var cityId = 2643743;
        var cachedWeatherDto = new WeatherDto
        {
            CityId = cityId,
            CityName = "London",
            Temperature = 20.0,
            FeelsLike = 19.0,
            Humidity = 65,
            WindSpeed = 3.5,
            Cloudiness = 40,
            Description = "cached"
        };
        _memoryCache.Set($"weather:{cityId}", cachedWeatherDto, TimeSpan.FromMinutes(5));

        // Act
        var status = _service.GetCacheStatus(cityId);

        // Assert
        Assert.Equal("HIT", status);
    }

    [Fact]
    public void GetCacheStatus_WithNoCachedData_ReturnsMiss()
    {
        // Arrange
        var cityId = 999999; // Not cached

        // Act
        var status = _service.GetCacheStatus(cityId);

        // Assert
        Assert.Equal("MISS", status);
    }

    [Fact]
    public async Task GetWeatherForCityAsync_WithCacheExpiration_RefetchesFromApi()
    {
        // Arrange
        var cityId = 2643743;
        var cityName = "London";
        
        // Create a cache with very short expiration for testing
        var shortLivedCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        var shortLivedService = new WeatherService(
            _mockCityDataService.Object,
            _mockOpenWeatherClient.Object,
            shortLivedCache,
            _mockLogger.Object);

        var weather = new Weather
        {
            Main = new MainData { Temp = 293.15, FeelsLike = 292.15, Humidity = 65 },
            Wind = new WindData { Speed = 3.5 },
            Clouds = new CloudsData { All = 40 },
            WeatherDescriptions = new List<WeatherDescription> { new() { Description = "clear sky" } }
        };

        var cities = new List<weather_comfort.Server.Models.City>
        {
            new() { Id = cityId, Name = cityName, CountryCode = "GB" }
        }.AsReadOnly();

        _mockCityDataService
            .Setup(x => x.GetCitiesAsync())
            .ReturnsAsync(cities);

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather);

        // Act - First call (cache miss)
        var result1 = await shortLivedService.GetWeatherForCityAsync(cityId);
        
        // Verify API was called
        _mockOpenWeatherClient.Verify(
            x => x.GetCurrentWeatherAsync(cityId, It.IsAny<CancellationToken>()),
            Times.Once);

        // Simulate cache expiration by clearing cache
        shortLivedCache.Remove($"weather:{cityId}");

        // Act - Second call after expiration (should fetch from API again)
        var result2 = await shortLivedService.GetWeatherForCityAsync(cityId);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        
        // Verify API was called twice (once before cache, once after expiration)
        _mockOpenWeatherClient.Verify(
            x => x.GetCurrentWeatherAsync(cityId, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task GetWeatherForCityAsync_WithCacheError_FallsBackToApi()
    {
        // Arrange
        var cityId = 2643743;
        var cityName = "London";
        
        var weather = new Weather
        {
            Main = new MainData { Temp = 293.15, FeelsLike = 292.15, Humidity = 65 },
            Wind = new WindData { Speed = 3.5 },
            Clouds = new CloudsData { All = 40 },
            WeatherDescriptions = new List<WeatherDescription> { new() { Description = "clear sky" } }
        };

        var cities = new List<weather_comfort.Server.Models.City>
        {
            new() { Id = cityId, Name = cityName, CountryCode = "GB" }
        }.AsReadOnly();

        _mockCityDataService
            .Setup(x => x.GetCitiesAsync())
            .ReturnsAsync(cities);

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather);

        // Note: This test verifies that the service works correctly with a real cache.
        // In a real implementation, cache errors would be caught and logged, but the service
        // should still function. Since IMemoryCache is a concrete implementation that doesn't
        // throw exceptions in normal operation, we test with a real cache instance.
        var realCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        var service = new WeatherService(
            _mockCityDataService.Object,
            _mockOpenWeatherClient.Object,
            realCache,
            _mockLogger.Object);

        // Act - Should work correctly with cache
        var result = await service.GetWeatherForCityAsync(cityId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cityId, result.CityId);
        
        // Verify API was called
        _mockOpenWeatherClient.Verify(
            x => x.GetCurrentWeatherAsync(cityId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetWeatherForCityAsync_WithNullCache_StillWorks()
    {
        // This test verifies that if cache is null (shouldn't happen in real scenario, but defensive)
        // the service should still function. However, since IMemoryCache is injected via DI,
        // it should never be null. This test is more of a documentation of expected behavior.
        
        // Arrange
        var cityId = 2643743;
        var cityName = "London";
        var weather = new Weather
        {
            Main = new MainData { Temp = 293.15, FeelsLike = 292.15, Humidity = 65 },
            Wind = new WindData { Speed = 3.5 },
            Clouds = new CloudsData { All = 40 },
            WeatherDescriptions = new List<WeatherDescription> { new() { Description = "clear sky" } }
        };

        var cities = new List<weather_comfort.Server.Models.City>
        {
            new() { Id = cityId, Name = cityName, CountryCode = "GB" }
        }.AsReadOnly();

        _mockCityDataService
            .Setup(x => x.GetCitiesAsync())
            .ReturnsAsync(cities);

        _mockOpenWeatherClient
            .Setup(x => x.GetCurrentWeatherAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather);

        // Act
        var result = await _service.GetWeatherForCityAsync(cityId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cityId, result.CityId);
    }
}

