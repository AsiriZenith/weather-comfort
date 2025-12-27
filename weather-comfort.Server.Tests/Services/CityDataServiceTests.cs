using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using weather_comfort.Server.Models;
using weather_comfort.Server.Services;
using Xunit;

namespace weather_comfort.Server.Tests.Services;

public class CityDataServiceTests
{
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly Mock<ILogger<CityDataService>> _mockLogger;
    private readonly CityDataService _service;

    public CityDataServiceTests()
    {
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockLogger = new Mock<ILogger<CityDataService>>();
        _service = new CityDataService(_mockEnvironment.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetCitiesAsync_WithValidJsonFile_ReturnsCities()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var configDir = Path.Combine(tempDir, "Config");
        Directory.CreateDirectory(configDir);
        var testFilePath = Path.Combine(configDir, "cities.json");
        var validCitiesJson = @"[
            {""id"": 2643743, ""name"": ""London"", ""countryCode"": ""GB""},
            {""id"": 1850147, ""name"": ""Tokyo"", ""countryCode"": ""JP""},
            {""id"": 2988507, ""name"": ""Paris"", ""countryCode"": ""FR""},
            {""id"": 2147714, ""name"": ""Sydney"", ""countryCode"": ""AU""},
            {""id"": 4930956, ""name"": ""Boston"", ""countryCode"": ""US""},
            {""id"": 1796236, ""name"": ""Shanghai"", ""countryCode"": ""CN""},
            {""id"": 3143244, ""name"": ""Oslo"", ""countryCode"": ""NO""},
            {""id"": 1248991, ""name"": ""Colombo"", ""countryCode"": ""LK""},
            {""id"": 2644210, ""name"": ""Liverpool"", ""countryCode"": ""GB""},
            {""id"": 5128581, ""name"": ""New York"", ""countryCode"": ""US""},
            {""id"": 1816670, ""name"": ""Beijing"", ""countryCode"": ""CN""},
            {""id"": 2950159, ""name"": ""Berlin"", ""countryCode"": ""DE""}
        ]";

        await File.WriteAllTextAsync(testFilePath, validCitiesJson);
        _mockEnvironment.Setup(e => e.ContentRootPath).Returns(tempDir);

        // Act
        var result = await _service.GetCitiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(12, result.Count);
        Assert.Contains(result, c => c.Id == 2643743 && c.Name == "London" && c.CountryCode == "GB");
        Assert.Contains(result, c => c.Id == 1850147 && c.Name == "Tokyo" && c.CountryCode == "JP");

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task GetCitiesAsync_WithMissingFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        _mockEnvironment.Setup(e => e.ContentRootPath).Returns(tempDir);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _service.GetCitiesAsync());

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task GetCitiesAsync_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var configDir = Path.Combine(tempDir, "Config");
        Directory.CreateDirectory(configDir);
        var testFilePath = Path.Combine(configDir, "cities.json");
        var invalidJson = "{ invalid json }";

        await File.WriteAllTextAsync(testFilePath, invalidJson);
        _mockEnvironment.Setup(e => e.ContentRootPath).Returns(tempDir);

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => _service.GetCitiesAsync());

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task GetCitiesAsync_WithLessThan10Cities_ThrowsInvalidOperationException()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var configDir = Path.Combine(tempDir, "Config");
        Directory.CreateDirectory(configDir);
        var testFilePath = Path.Combine(configDir, "cities.json");
        var insufficientCitiesJson = @"[
            {""id"": 2643743, ""name"": ""London"", ""countryCode"": ""GB""},
            {""id"": 1850147, ""name"": ""Tokyo"", ""countryCode"": ""JP""},
            {""id"": 2988507, ""name"": ""Paris"", ""countryCode"": ""FR""}
        ]";

        await File.WriteAllTextAsync(testFilePath, insufficientCitiesJson);
        _mockEnvironment.Setup(e => e.ContentRootPath).Returns(tempDir);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetCitiesAsync());
        Assert.Contains("10", exception.Message);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task GetCitiesAsync_WithExactly10Cities_ReturnsCities()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var configDir = Path.Combine(tempDir, "Config");
        Directory.CreateDirectory(configDir);
        var testFilePath = Path.Combine(configDir, "cities.json");
        var exactly10CitiesJson = @"[
            {""id"": 2643743, ""name"": ""London"", ""countryCode"": ""GB""},
            {""id"": 1850147, ""name"": ""Tokyo"", ""countryCode"": ""JP""},
            {""id"": 2988507, ""name"": ""Paris"", ""countryCode"": ""FR""},
            {""id"": 2147714, ""name"": ""Sydney"", ""countryCode"": ""AU""},
            {""id"": 4930956, ""name"": ""Boston"", ""countryCode"": ""US""},
            {""id"": 1796236, ""name"": ""Shanghai"", ""countryCode"": ""CN""},
            {""id"": 3143244, ""name"": ""Oslo"", ""countryCode"": ""NO""},
            {""id"": 1248991, ""name"": ""Colombo"", ""countryCode"": ""LK""},
            {""id"": 2644210, ""name"": ""Liverpool"", ""countryCode"": ""GB""},
            {""id"": 5128581, ""name"": ""New York"", ""countryCode"": ""US""}
        ]";

        await File.WriteAllTextAsync(testFilePath, exactly10CitiesJson);
        _mockEnvironment.Setup(e => e.ContentRootPath).Returns(tempDir);

        // Act
        var result = await _service.GetCitiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Count);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task GetCitiesAsync_WithNullDeserializedResult_ThrowsJsonException()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var configDir = Path.Combine(tempDir, "Config");
        Directory.CreateDirectory(configDir);
        var testFilePath = Path.Combine(configDir, "cities.json");
        var nullResultJson = "null";

        await File.WriteAllTextAsync(testFilePath, nullResultJson);
        _mockEnvironment.Setup(e => e.ContentRootPath).Returns(tempDir);

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => _service.GetCitiesAsync());

        // Cleanup
        Directory.Delete(tempDir, true);
    }
}

