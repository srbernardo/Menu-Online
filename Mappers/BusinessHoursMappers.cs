using MenuOnline.Dtos.BusinessHours;
using MenuOnline.Models;

namespace MenuOnline.Mappers;

public static class BusinessHoursMappers
{
  public static BusinessHoursDto ToBusinessHoursDto(this BusinessHours model)
  {
    return new BusinessHoursDto
    (
      model.Id,
      model.Monday,
      model.Tuesday,
      model.Wednesday,
      model.Thursday,
      model.Friday,
      model.Saturday,
      model.Sunday
    );
  }

  public static BusinessHours ToBusinessHoursFromDto(this UpsertBusinessHoursRequestDto dto)
  {
    var model = new BusinessHours();
    dto.ApplyTo(model);
    return model;
  }

  public static void ApplyTo(this UpsertBusinessHoursRequestDto dto, BusinessHours model)
  {
    model.Monday = dto.Monday ?? "";
    model.Tuesday = dto.Tuesday ?? "";
    model.Wednesday = dto.Wednesday ?? "";
    model.Thursday = dto.Thursday ?? "";
    model.Friday = dto.Friday ?? "";
    model.Saturday = dto.Saturday ?? "";
    model.Sunday = dto.Sunday ?? "";
  }
}
