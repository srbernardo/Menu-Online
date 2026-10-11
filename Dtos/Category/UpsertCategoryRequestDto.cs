using System.ComponentModel.DataAnnotations;

namespace MenuOnline.Dtos.Category;

public record UpsertCategoryRequestDto
(
  [Required, MinLength(1)] string Title
);
