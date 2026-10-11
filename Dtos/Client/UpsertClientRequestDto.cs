using System.ComponentModel.DataAnnotations;

namespace MenuOnline.Dtos.Client;

public record UpsertClientRequestDto
(
  [Required] string Name,
  [Required] string WhatsappNumber,
  string? Email,
  DateOnly BirthDate
);
