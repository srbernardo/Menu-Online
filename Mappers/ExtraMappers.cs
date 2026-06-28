using System;
using MenuOnline.Dtos.Extra;
using MenuOnline.Models;

namespace MenuOnline.Mappers;

public static class ExtraMappers
{
  public static ExtraDto ToExtraDto(this Extra extraModel)
  {
    return new ExtraDto
    (
      extraModel.Id,
      extraModel.Description,
      extraModel.Title,
      extraModel.Value
    );
  }

  public static Extra ToExtraFromCreateDto(this UpsertExtraRequestDto dto)
  {
    return new Extra
    {
      Description = dto.Description,
      Title = dto.Title,
      Value = dto.Value,
      UserId = dto.UserId
    };
  }
}
