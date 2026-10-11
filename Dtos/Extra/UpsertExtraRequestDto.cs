using System.ComponentModel.DataAnnotations;

namespace MenuOnline.Dtos.Extra;

public record UpsertExtraRequestDto
(
  [Required, MinLength(1)] string Title,
  string Description,
  [Range(0, 100000)] decimal Value
);
