using System;
using MenuOnline.Models;

namespace MenuOnline.Dtos.Item;

public record UpsetItemRequestDto
(
  int CategoryId,
  string Description,
  List<int> ExtraIds,
  string ImageUrl,
  decimal Price,
  string Title,
  int UserId
);
