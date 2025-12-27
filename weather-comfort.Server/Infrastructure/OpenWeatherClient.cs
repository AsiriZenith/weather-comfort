using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using weather_comfort.Server.Models;

namespace weather_comfort.Server.Infrastructure;

public class OpenWeatherClient : IOpenWeatherClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenWeatherClient> _logger;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";

    public OpenWeatherClient(HttpClient httpClient, IConfiguration configuration, ILogger<OpenWeatherClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["api_key"] ?? throw new InvalidOperationException("OpenWeatherMap API key not found in configuration. Please set 'api_key' in appsettings.json or environment variables.");
    }

    public async Task<Weather> GetCurrentWeatherAsync(int cityId, CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}?id={cityId}&appid={_apiKey}";

        try
        {
            _logger.LogInformation("Fetching weather data for city ID: {CityId}", cityId);

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OpenWeatherMap API returned error status {StatusCode} for city ID {CityId}. Response: {ErrorContent}",
                    response.StatusCode, cityId, errorContent);

                throw new HttpRequestException(
                    $"OpenWeatherMap API returned status code {response.StatusCode} for city ID {cityId}. Response: {errorContent}",
                    null, response.StatusCode);
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var weather = await response.Content.ReadFromJsonAsync<Weather>(jsonOptions, cancellationToken);

            if (weather == null)
            {
                _logger.LogError("Failed to deserialize weather response for city ID {CityId} - result is null", cityId);
                throw new JsonException($"Failed to deserialize weather response for city ID {cityId} - result is null");
            }

            _logger.LogInformation("Successfully retrieved weather data for city ID: {CityId}", cityId);
            return weather;
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Request timeout while fetching weather data for city ID {CityId}", cityId);
            throw new HttpRequestException($"Request timeout while fetching weather data for city ID {cityId}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request cancelled while fetching weather data for city ID {CityId}", cityId);
            throw new OperationCanceledException($"Request cancelled while fetching weather data for city ID {cityId}", ex, cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse weather response as valid JSON for city ID {CityId}", cityId);
            throw new JsonException($"Invalid JSON format in weather response for city ID {cityId}: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching weather data for city ID {CityId}", cityId);
            throw;
        }
    }
}
