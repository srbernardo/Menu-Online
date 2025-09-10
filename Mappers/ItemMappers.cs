using System;
using MenuOnline.Data;
using MenuOnline.Dtos.Item;
using MenuOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Mappers;

public static class ItemMappers
{
  public static ItemDto ToItemDto(this Item itemModel, AppDbContext context)
  {
    var categoryName = context.Categories.AsNoTracking().FirstOrDefault(c => c.Id == itemModel.CategoryId)?.Title ?? "";
    var extras = itemModel.Extras.Select(e => e.ToExtraDto()).ToList();

    return new ItemDto
    (
      itemModel.Id,
      categoryName,
      itemModel.Description,
      itemModel.Extras.Select(e => e.ToExtraDto()).ToList(),
      itemModel.ImageUrl,
      itemModel.Price,
      itemModel.Title
    );
  }

  public static Item ToItemFromCreateDto(this UpsetItemRequestDto ItemDto)
  {
    return new Item
    {
      CategoryId = ItemDto.CategoryId,
      Description = ItemDto.Description,
      ImageUrl = ItemDto.ImageUrl,
      Price = ItemDto.Price,
      Title = ItemDto.Title,
      UserId = ItemDto.UserId
    };
  }
}
