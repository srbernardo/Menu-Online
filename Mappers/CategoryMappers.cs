using MenuOnline.Dtos.Category;
using MenuOnline.Models;

namespace MenuOnline.Mappers;

public static class CategoryMappers
{
    public static CategoryDto ToCategoryDto(this Category category)
    {
        return new CategoryDto(category.Id, category.Title);
    }

    public static Category ToCategoryFromCreateDto(this UpsertCategoryRequestDto dto)
    {
        return new Category
        {
            Title = dto.Title,
            UserId = dto.UserId
        };
    }
}
