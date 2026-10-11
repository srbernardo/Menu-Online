using System.ComponentModel.DataAnnotations;

namespace MenuOnline.Dtos.Item;

public record UpsertItemRequestDto
(
  int CategoryId,
  string? Description,
  List<int>? ExtraIds,
  string? ImageUrl,
  [Range(0, 100000)] decimal Price,
  [Required, MinLength(1)] string Title,
  int UserId
);
