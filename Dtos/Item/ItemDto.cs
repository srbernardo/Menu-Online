using MenuOnline.Dtos.Extra;

namespace MenuOnline.Dtos.Item;

public record ItemDto
(
  int Id,
  string CategoryName,
  string Description,
  List<ExtraDto> Extras,
  string ImageUrl,
  decimal Price,
  string Title
);
