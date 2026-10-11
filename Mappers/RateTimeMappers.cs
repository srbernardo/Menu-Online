using MenuOnline.Dtos.RateTime;
using MenuOnline.Models;

namespace MenuOnline.Mappers;

public static class RateTimeMappers
{
  public static RateTimeDto ToRateTimeDto(this RateTime model)
  {
    return new RateTimeDto(model.Id, model.TimeInMinutes, model.Km, model.Rate);
  }

  public static RateTime ToRateTimeFromDto(this UpsertRateTimeRequestDto dto, int userId)
  {
    return new RateTime
    {
      TimeInMinutes = dto.TimeInMinutes,
      Km = dto.Km,
      Rate = dto.Rate,
      UserId = userId
    };
  }
}
