using MenuOnline.Dtos.BusinessHours;
using MenuOnline.Models;

namespace MenuOnline.Mappers;

public static class BusinessHoursMappers
{
    public static BusinessHoursDto ToBusinessHoursDto(this BusinessHours bh)
    {
        return new BusinessHoursDto(
            bh.Id,
            bh.UserId,
            bh.Monday,
            bh.Tuesday,
            bh.Wednesday,
            bh.Thursday,
            bh.Friday,
            bh.Saturday,
            bh.Sunday
        );
    }

    public static BusinessHours ToBusinessHoursFromCreateDto(this UpsertBusinessHoursRequestDto dto)
    {
        return new BusinessHours
        {
            UserId = dto.UserId,
            Monday = dto.Monday,
            Tuesday = dto.Tuesday,
            Wednesday = dto.Wednesday,
            Thursday = dto.Thursday,
            Friday = dto.Friday,
            Saturday = dto.Saturday,
            Sunday = dto.Sunday
        };
    }
}
