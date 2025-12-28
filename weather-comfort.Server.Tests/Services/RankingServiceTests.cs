using Microsoft.Extensions.Logging;
using Moq;
using weather_comfort.Server.DTOs;
using weather_comfort.Server.Services;
using Xunit;

namespace weather_comfort.Server.Tests.Services;

public class RankingServiceTests
{
    private readonly Mock<ILogger<RankingService>> _mockLogger;
    private readonly RankingService _service;

    public RankingServiceTests()
    {
        _mockLogger = new Mock<ILogger<RankingService>>();
        _service = new RankingService(_mockLogger.Object);
    }

    [Fact]
    public void RankCities_WithMultipleCitiesDifferentScores_HighestScoreGetsRank1()
    {
        // Arrange
        var comfortIndices = new List<ComfortIndexDto>
        {
            new() { CityId = 1, CityName = "City A", Score = 85.0 },
            new() { CityId = 2, CityName = "City B", Score = 95.0 },
            new() { CityId = 3, CityName = "City C", Score = 75.0 },
            new() { CityId = 4, CityName = "City D", Score = 90.0 }
        };

        // Act
        var result = _service.RankCities(comfortIndices);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        
        // Highest score (95.0) should be rank 1
        var rank1 = result.First(r => r.Rank == 1);
        Assert.Equal(95.0, rank1.Score);
        Assert.Equal(2, rank1.CityId);
        Assert.Equal("City B", rank1.CityName);
        
        // Verify descending order
        Assert.Equal(95.0, result[0].Score);
        Assert.Equal(90.0, result[1].Score);
        Assert.Equal(85.0, result[2].Score);
        Assert.Equal(75.0, result[3].Score);
    }

    [Fact]
    public void RankCities_WithTiedScores_SameRankAssigned()
    {
        // Arrange
        var comfortIndices = new List<ComfortIndexDto>
        {
            new() { CityId = 1, CityName = "City A", Score = 95.0 },
            new() { CityId = 2, CityName = "City B", Score = 95.0 },
            new() { CityId = 3, CityName = "City C", Score = 90.0 }
        };

        // Act
        var result = _service.RankCities(comfortIndices);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        
        // Both cities with score 95.0 should have rank 1
        var rank1Cities = result.Where(r => r.Rank == 1).ToList();
        Assert.Equal(2, rank1Cities.Count);
        Assert.All(rank1Cities, r => Assert.Equal(95.0, r.Score));
        
        // Next city should have rank 3 (competition ranking - skips rank 2)
        var rank3City = result.First(r => r.Rank == 3);
        Assert.Equal(90.0, rank3City.Score);
        Assert.Equal(3, rank3City.CityId);
    }

    [Fact]
    public void RankCities_WithTiedScores_NextRankSkipsAppropriately()
    {
        // Arrange - Two cities tie for rank 1, next should be rank 3
        var comfortIndices = new List<ComfortIndexDto>
        {
            new() { CityId = 1, CityName = "City A", Score = 95.0 },
            new() { CityId = 2, CityName = "City B", Score = 95.0 },
            new() { CityId = 3, CityName = "City C", Score = 90.0 },
            new() { CityId = 4, CityName = "City D", Score = 85.0 }
        };

        // Act
        var result = _service.RankCities(comfortIndices);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        
        // Verify competition ranking: rank 1 (tied), rank 3, rank 4
        var ranks = result.Select(r => r.Rank).ToList();
        Assert.Contains(1, ranks);
        Assert.DoesNotContain(2, ranks); // Rank 2 should be skipped
        Assert.Contains(3, ranks);
        Assert.Contains(4, ranks);
        
        // Verify two cities have rank 1
        Assert.Equal(2, result.Count(r => r.Rank == 1));
        
        // Verify rank 3 city
        var rank3City = result.First(r => r.Rank == 3);
        Assert.Equal(90.0, rank3City.Score);
    }

    [Fact]
    public void RankCities_WithSingleCity_GetsRank1()
    {
        // Arrange
        var comfortIndices = new List<ComfortIndexDto>
        {
            new() { CityId = 1, CityName = "City A", Score = 85.0 }
        };

        // Act
        var result = _service.RankCities(comfortIndices);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result[0].Rank);
        Assert.Equal(1, result[0].CityId);
        Assert.Equal("City A", result[0].CityName);
        Assert.Equal(85.0, result[0].Score);
    }

    [Fact]
    public void RankCities_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var comfortIndices = new List<ComfortIndexDto>();

        // Act
        var result = _service.RankCities(comfortIndices);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void RankCities_WithNullInput_ReturnsEmptyList()
    {
        // Arrange
        IReadOnlyList<ComfortIndexDto>? comfortIndices = null;

        // Act
        var result = _service.RankCities(comfortIndices!);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void RankCities_WithAllSameScores_AllGetRank1()
    {
        // Arrange
        var comfortIndices = new List<ComfortIndexDto>
        {
            new() { CityId = 1, CityName = "City A", Score = 85.0 },
            new() { CityId = 2, CityName = "City B", Score = 85.0 },
            new() { CityId = 3, CityName = "City C", Score = 85.0 }
        };

        // Act
        var result = _service.RankCities(comfortIndices);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.All(result, r => Assert.Equal(1, r.Rank));
        Assert.All(result, r => Assert.Equal(85.0, r.Score));
    }

    [Fact]
    public void RankCities_WithNewData_RecalculatesRankings()
    {
        // Arrange - First dataset
        var firstDataset = new List<ComfortIndexDto>
        {
            new() { CityId = 1, CityName = "City A", Score = 95.0 },
            new() { CityId = 2, CityName = "City B", Score = 85.0 }
        };

        // Act - First ranking
        var firstResult = _service.RankCities(firstDataset);

        // Assert - First ranking
        Assert.Equal(1, firstResult[0].Rank);
        Assert.Equal(95.0, firstResult[0].Score);
        Assert.Equal(2, firstResult[1].Rank);
        Assert.Equal(85.0, firstResult[1].Score);

        // Arrange - Second dataset with changed scores
        var secondDataset = new List<ComfortIndexDto>
        {
            new() { CityId = 1, CityName = "City A", Score = 80.0 },
            new() { CityId = 2, CityName = "City B", Score = 90.0 }
        };

        // Act - Second ranking
        var secondResult = _service.RankCities(secondDataset);

        // Assert - Rankings recalculated correctly
        Assert.Equal(1, secondResult[0].Rank);
        Assert.Equal(90.0, secondResult[0].Score); // City B now has highest score
        Assert.Equal(2, secondResult[1].Rank);
        Assert.Equal(80.0, secondResult[1].Score); // City A now has lower score
    }

    [Fact]
    public void RankCities_WithDescendingOrder_HighestScoreFirst()
    {
        // Arrange
        var comfortIndices = new List<ComfortIndexDto>
        {
            new() { CityId = 1, CityName = "City A", Score = 70.0 },
            new() { CityId = 2, CityName = "City B", Score = 90.0 },
            new() { CityId = 3, CityName = "City C", Score = 80.0 },
            new() { CityId = 4, CityName = "City D", Score = 100.0 }
        };

        // Act
        var result = _service.RankCities(comfortIndices);

        // Assert - Verify descending order: highest score first
        Assert.Equal(100.0, result[0].Score);
        Assert.Equal(90.0, result[1].Score);
        Assert.Equal(80.0, result[2].Score);
        Assert.Equal(70.0, result[3].Score);
        
        // Verify ranks are sequential (no ties in this case)
        Assert.Equal(1, result[0].Rank);
        Assert.Equal(2, result[1].Rank);
        Assert.Equal(3, result[2].Rank);
        Assert.Equal(4, result[3].Rank);
    }

    [Fact]
    public void RankCities_WithSequentialRanks_NoGapsExceptForTies()
    {
        // Arrange - No ties, should have sequential ranks 1, 2, 3, 4
        var comfortIndices = new List<ComfortIndexDto>
        {
            new() { CityId = 1, CityName = "City A", Score = 100.0 },
            new() { CityId = 2, CityName = "City B", Score = 90.0 },
            new() { CityId = 3, CityName = "City C", Score = 80.0 },
            new() { CityId = 4, CityName = "City D", Score = 70.0 }
        };

        // Act
        var result = _service.RankCities(comfortIndices);

        // Assert - Ranks should be sequential: 1, 2, 3, 4
        var ranks = result.Select(r => r.Rank).OrderBy(r => r).ToList();
        Assert.Equal(new[] { 1, 2, 3, 4 }, ranks);
    }

    [Fact]
    public void RankCities_PreservesPenaltyBreakdown()
    {
        // Arrange
        var comfortIndices = new List<ComfortIndexDto>
        {
            new()
            {
                CityId = 1,
                CityName = "City A",
                Score = 95.0,
                TemperaturePenalty = 2.5,
                HumidityPenalty = 1.0,
                WindPenalty = 0.5,
                CloudinessPenalty = 1.0
            },
            new()
            {
                CityId = 2,
                CityName = "City B",
                Score = 85.0,
                TemperaturePenalty = 5.0,
                HumidityPenalty = 3.0,
                WindPenalty = 2.0,
                CloudinessPenalty = 5.0
            }
        };

        // Act
        var result = _service.RankCities(comfortIndices);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var cityA = result.First(r => r.CityId == 1);
        Assert.Equal(2.5, cityA.TemperaturePenalty);
        Assert.Equal(1.0, cityA.HumidityPenalty);
        Assert.Equal(0.5, cityA.WindPenalty);
        Assert.Equal(1.0, cityA.CloudinessPenalty);
        
        var cityB = result.First(r => r.CityId == 2);
        Assert.Equal(5.0, cityB.TemperaturePenalty);
        Assert.Equal(3.0, cityB.HumidityPenalty);
        Assert.Equal(2.0, cityB.WindPenalty);
        Assert.Equal(5.0, cityB.CloudinessPenalty);
    }

    [Fact]
    public void RankCities_WithComplexTies_HandlesCorrectly()
    {
        // Arrange - Multiple groups of ties
        var comfortIndices = new List<ComfortIndexDto>
        {
            new() { CityId = 1, CityName = "City A", Score = 95.0 },
            new() { CityId = 2, CityName = "City B", Score = 95.0 },
            new() { CityId = 3, CityName = "City C", Score = 90.0 },
            new() { CityId = 4, CityName = "City D", Score = 90.0 },
            new() { CityId = 5, CityName = "City E", Score = 90.0 },
            new() { CityId = 6, CityName = "City F", Score = 85.0 }
        };

        // Act
        var result = _service.RankCities(comfortIndices);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.Count);
        
        // Two cities with 95.0 should have rank 1
        Assert.Equal(2, result.Count(r => r.Rank == 1 && r.Score == 95.0));
        
        // Three cities with 90.0 should have rank 3 (skips rank 2)
        Assert.Equal(3, result.Count(r => r.Rank == 3 && r.Score == 90.0));
        
        // One city with 85.0 should have rank 6 (skips ranks 4 and 5)
        Assert.Single(result, r => r.Rank == 6 && r.Score == 85.0);
    }
}

