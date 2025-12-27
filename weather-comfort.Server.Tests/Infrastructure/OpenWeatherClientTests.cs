using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using weather_comfort.Server.Infrastructure;
using weather_comfort.Server.Models;
using Xunit;

namespace weather_comfort.Server.Tests.Infrastructure;

public class OpenWeatherClientTests
{
    private readonly Mock<ILogger<OpenWeatherClient>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly string _apiKey = "test-api-key";

    public OpenWeatherClientTests()
    {
        _mockLogger = new Mock<ILogger<OpenWeatherClient>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["api_key"]).Returns(_apiKey);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WithValidResponse_ReturnsWeather()
    {
        // Arrange
        var expectedWeather = new Weather
        {
            Main = new MainData
            {
                Temp = 293.15, // 20°C in Kelvin
                FeelsLike = 292.15,
                Humidity = 65
            },
            Wind = new WindData { Speed = 3.5 },
            Clouds = new CloudsData { All = 40 },
            WeatherDescriptions = new List<WeatherDescription>
            {
                new() { Description = "clear sky" }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(new
        {
            main = new { temp = 293.15, feels_like = 292.15, humidity = 65 },
            wind = new { speed = 3.5 },
            clouds = new { all = 40 },
            weather = new[] { new { description = "clear sky" } }
        });

        var httpClient = CreateHttpClient(HttpStatusCode.OK, jsonResponse);
        var client = new OpenWeatherClient(httpClient, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await client.GetCurrentWeatherAsync(2643743);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Main);
        Assert.Equal(293.15, result.Main.Temp);
        Assert.Equal(292.15, result.Main.FeelsLike);
        Assert.Equal(65, result.Main.Humidity);
        Assert.NotNull(result.Wind);
        Assert.Equal(3.5, result.Wind.Speed);
        Assert.NotNull(result.Clouds);
        Assert.Equal(40, result.Clouds.All);
        Assert.NotNull(result.WeatherDescriptions);
        Assert.Single(result.WeatherDescriptions);
        Assert.Equal("clear sky", result.WeatherDescriptions[0].Description);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WithHttpError_ThrowsHttpRequestException()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.NotFound, "City not found");
        var client = new OpenWeatherClient(httpClient, _mockConfiguration.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => client.GetCurrentWeatherAsync(999999));
        Assert.True(exception.Message.Contains("404") || exception.Message.Contains("NotFound") || 
                   exception.Data.Contains("StatusCode"), 
                   $"Exception message should contain status code info. Actual: {exception.Message}");
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WithUnauthorized_ThrowsHttpRequestException()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.Unauthorized, "Invalid API key");
        var client = new OpenWeatherClient(httpClient, _mockConfiguration.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => client.GetCurrentWeatherAsync(2643743));
        Assert.True(exception.Message.Contains("401") || exception.Message.Contains("Unauthorized") || 
                   exception.Data.Contains("StatusCode"), 
                   $"Exception message should contain status code info. Actual: {exception.Message}");
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WithRateLimit_ThrowsHttpRequestException()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.TooManyRequests, "Rate limit exceeded");
        var client = new OpenWeatherClient(httpClient, _mockConfiguration.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => client.GetCurrentWeatherAsync(2643743));
        Assert.True(exception.Message.Contains("429") || exception.Message.Contains("TooManyRequests") || 
                   exception.Data.Contains("StatusCode"), 
                   $"Exception message should contain status code info. Actual: {exception.Message}");
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WithServerError_ThrowsHttpRequestException()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.InternalServerError, "Internal server error");
        var client = new OpenWeatherClient(httpClient, _mockConfiguration.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => client.GetCurrentWeatherAsync(2643743));
        Assert.True(exception.Message.Contains("500") || exception.Message.Contains("InternalServerError") || 
                   exception.Data.Contains("StatusCode"), 
                   $"Exception message should contain status code info. Actual: {exception.Message}");
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.OK, "{ invalid json }");
        var client = new OpenWeatherClient(httpClient, _mockConfiguration.Object, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => client.GetCurrentWeatherAsync(2643743));
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WithNullResponse_ThrowsJsonException()
    {
        // Arrange
        var httpClient = CreateHttpClient(HttpStatusCode.OK, "null");
        var client = new OpenWeatherClient(httpClient, _mockConfiguration.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<JsonException>(() => client.GetCurrentWeatherAsync(2643743));
        Assert.Contains("null", exception.Message);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WithTimeout_ThrowsHttpRequestException()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout", new TimeoutException()));

        var httpClient = new HttpClient(handler.Object)
        {
            Timeout = TimeSpan.FromSeconds(1)
        };

        var client = new OpenWeatherClient(httpClient, _mockConfiguration.Object, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetCurrentWeatherAsync(2643743));
    }

    [Fact]
    public void Constructor_WithMissingApiKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["api_key"]).Returns((string?)null);
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            new OpenWeatherClient(httpClient, mockConfig.Object, _mockLogger.Object));
    }

    private HttpClient CreateHttpClient(HttpStatusCode statusCode, string content)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });

        return new HttpClient(handler.Object);
    }
}

