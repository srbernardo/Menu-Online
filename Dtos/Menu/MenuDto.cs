using MenuOnline.Dtos.Address;
using MenuOnline.Dtos.Extra;

namespace MenuOnline.Dtos.Menu;

// Contrato do endpoint agregado GET /api/menu/{slug}.
// Espelha src/types/menu.ts do front-end (menu-online-web).
public record MenuItemDto
(
  int Id,
  string Title,
  string Description,
  string? ImageUrl,
  decimal Price,
  List<ExtraDto> Extras
);

public record MenuCategoryDto
(
  int Id,
  string Title,
  List<MenuItemDto> Items
);

public record MenuRestaurantDto
(
  string Slug,
  string Name,
  string Description,
  string? CoverUrl,
  string? LogoUrl,
  bool IsOpen,
  decimal DeliveryFee,
  string? Phone,
  string? Whatsapp,
  AddressDto? Address
);

public record RestaurantMenuDto
(
  MenuRestaurantDto Restaurant,
  List<MenuCategoryDto> Categories
);
