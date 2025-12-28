using weather_comfort.Server.DTOs;

namespace weather_comfort.Server.Services;

public interface IRankingService
{
    IReadOnlyList<RankedCityDto> RankCities(IReadOnlyList<ComfortIndexDto> comfortIndices);
}

