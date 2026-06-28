using MenuOnline.Dtos.RateTime;
using MenuOnline.Models;

namespace MenuOnline.Mappers;

public static class RateTimeMappers
{
    public static RateTimeDto ToRateTimeDto(this RateTime rt)
    {
        return new RateTimeDto(rt.Id, rt.Km, rt.TimeInMinutes, rt.Rate, rt.UserId);
    }

    public static RateTime ToRateTimeFromCreateDto(this UpsertRateTimeRequestDto dto)
    {
        return new RateTime
        {
            Km = dto.Km,
            TimeInMinutes = dto.TimeInMinutes,
            Rate = dto.Rate,
            UserId = dto.UserId
        };
    }
}
