using System.Text.Json;
using weather_comfort.Server.Models;

namespace weather_comfort.Server.Services;

public class CityDataService(IWebHostEnvironment environment, ILogger<CityDataService> logger) : ICityDataService
{
    private const string CitiesFileName = "Config/cities.json";
    private const int MinimumCitiesRequired = 10;

    public async Task<IReadOnlyList<City>> GetCitiesAsync()
    {
        var filePath = Path.Combine(environment.ContentRootPath, CitiesFileName);

        if (!File.Exists(filePath))
        {
            logger.LogError("Cities file not found at path: {FilePath}", filePath);
            throw new FileNotFoundException($"Cities configuration file not found at: {filePath}", filePath);
        }

        try
        {
            var jsonContent = await File.ReadAllTextAsync(filePath);
            var cities = JsonSerializer.Deserialize<List<City>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (cities == null)
            {
                logger.LogError("Failed to deserialize cities.json - result is null");
                throw new JsonException("Failed to deserialize cities.json - result is null");
            }

            if (cities.Count < MinimumCitiesRequired)
            {
                logger.LogError("Cities file contains {Count} cities, but minimum {Minimum} cities are required", 
                    cities.Count, MinimumCitiesRequired);
                throw new InvalidOperationException(
                    $"Cities file must contain at least {MinimumCitiesRequired} cities. Found {cities.Count} cities.");
            }

            logger.LogInformation("Successfully loaded {Count} cities from cities.json", cities.Count);
            return cities.AsReadOnly();
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to parse cities.json as valid JSON");
            throw new JsonException($"Invalid JSON format in cities.json: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while reading cities.json");
            throw;
        }
    }
}

