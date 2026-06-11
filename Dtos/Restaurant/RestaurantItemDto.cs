using MenuOnline.Dtos.Extra;

namespace MenuOnline.Dtos.Restaurant;

public record RestaurantItemDto
(
  int Id,
  string Title,
  string Description,
  string ImageUrl,
  decimal Price,
  int CategoryId,
  string CategoryName,
  List<ExtraDto> Extras
);
