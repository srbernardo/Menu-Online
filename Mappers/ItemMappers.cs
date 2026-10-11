using MenuOnline.Dtos.Item;
using MenuOnline.Models;

namespace MenuOnline.Mappers;

public static class ItemMappers
{
  // Requer Category e ItemExtras.Extra carregados (Include).
  public static ItemDto ToItemDto(this Item itemModel)
  {
    return new ItemDto
    (
      itemModel.Id,
      itemModel.Category?.Title ?? "",
      itemModel.Description,
      itemModel.ItemExtras
        .Where(ie => ie.Extra != null)
        .Select(ie => ie.Extra!.ToExtraDto())
        .ToList(),
      itemModel.ImageUrl,
      itemModel.Price,
      itemModel.Title
    );
  }

  public static Item ToItemFromCreateDto(this UpsertItemRequestDto dto)
  {
    return new Item
    {
      CategoryId = dto.CategoryId,
      Description = dto.Description ?? "",
      ImageUrl = dto.ImageUrl ?? "",
      Price = dto.Price,
      Title = dto.Title,
      UserId = dto.UserId
    };
  }
}
