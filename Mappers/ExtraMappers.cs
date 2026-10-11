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

  public static Extra ToExtraFromDto(this UpsertExtraRequestDto dto)
  {
    return new Extra
    {
      Title = dto.Title,
      Description = dto.Description ?? "",
      Value = dto.Value
    };
  }
}
