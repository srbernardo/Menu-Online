using MenuOnline.Dtos.Menu;
using MenuOnline.Models;

namespace MenuOnline.Mappers;

public static class MenuMappers
{
  private static string? NullIfEmpty(string value) =>
    string.IsNullOrWhiteSpace(value) ? null : value;

  // Requer Address carregado no User; categorias com Items (da loja) e
  // ItemExtras.Extra carregados.
  public static RestaurantMenuDto ToMenuDto(this User user, List<Category> categories, decimal deliveryFee)
  {
    return new RestaurantMenuDto
    (
      new MenuRestaurantDto
      (
        user.Slug,
        user.Name,
        user.Description,
        NullIfEmpty(user.CoverUrl),
        NullIfEmpty(user.LogoUrl),
        user.IsOpen,
        deliveryFee,
        NullIfEmpty(user.Phone),
        NullIfEmpty(user.WhatsappNumber),
        user.Address?.ToAddressDto()
      ),
      categories.Select(c => new MenuCategoryDto
      (
        c.Id,
        c.Title,
        c.Items
          .OrderBy(i => i.Title)
          .Select(i => new MenuItemDto
          (
            i.Id,
            i.Title,
            i.Description,
            NullIfEmpty(i.ImageUrl),
            i.Price,
            i.ItemExtras
              .Where(ie => ie.Extra != null)
              .Select(ie => ie.Extra!.ToExtraDto())
              .ToList()
          ))
          .ToList()
      )).ToList()
    );
  }
}
