using weather_comfort.Server.DTOs;

namespace weather_comfort.Server.Services;

public class RankingService : IRankingService
{
    private readonly ILogger<RankingService> _logger;

    public RankingService(ILogger<RankingService> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<RankedCityDto> RankCities(IReadOnlyList<ComfortIndexDto> comfortIndices)
    {
        if (comfortIndices == null)
        {
            _logger.LogWarning("Null comfort indices list provided for ranking");
            return Array.Empty<RankedCityDto>().AsReadOnly();
        }

        if (comfortIndices.Count == 0)
        {
            _logger.LogInformation("Empty comfort indices list provided for ranking");
            return Array.Empty<RankedCityDto>().AsReadOnly();
        }

        if (comfortIndices.Count == 1)
        {
            var single = comfortIndices[0];
            return new List<RankedCityDto>
            {
                new RankedCityDto
                {
                    Rank = 1,
                    CityId = single.CityId,
                    CityName = single.CityName,
                    Score = single.Score,
                    TemperaturePenalty = single.TemperaturePenalty,
                    HumidityPenalty = single.HumidityPenalty,
                    WindPenalty = single.WindPenalty,
                    CloudinessPenalty = single.CloudinessPenalty
                }
            }.AsReadOnly();
        }

        try
        {
            var sorted = comfortIndices
                .OrderByDescending(ci => ci.Score)
                .ToList();

            var ranked = new List<RankedCityDto>();
            int currentRank = 1;
            int index = 0;

            while (index < sorted.Count)
            {
                var currentScore = sorted[index].Score;
                var tiedCount = 0;

                for (int j = index; j < sorted.Count && sorted[j].Score == currentScore; j++)
                {
                    tiedCount++;
                }

                for (int k = 0; k < tiedCount; k++)
                {
                    var comfortIndex = sorted[index + k];
                    ranked.Add(new RankedCityDto
                    {
                        Rank = currentRank,
                        CityId = comfortIndex.CityId,
                        CityName = comfortIndex.CityName,
                        Score = comfortIndex.Score,
                        TemperaturePenalty = comfortIndex.TemperaturePenalty,
                        HumidityPenalty = comfortIndex.HumidityPenalty,
                        WindPenalty = comfortIndex.WindPenalty,
                        CloudinessPenalty = comfortIndex.CloudinessPenalty
                    });
                }

                index += tiedCount;
                currentRank += tiedCount;
            }

            _logger.LogInformation("Ranked {Count} cities successfully", ranked.Count);
            return ranked.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ranking cities");
            return Array.Empty<RankedCityDto>().AsReadOnly();
        }
    }
}

