using MenuOnline.Dtos.Category;
using MenuOnline.Models;

namespace MenuOnline.Mappers;

public static class CategoryMappers
{
  public static CategoryDto ToCategoryDto(this Category model)
  {
    return new CategoryDto(model.Id, model.Title);
  }

  public static Category ToCategoryFromDto(this UpsertCategoryRequestDto dto)
  {
    return new Category { Title = dto.Title };
  }
}
