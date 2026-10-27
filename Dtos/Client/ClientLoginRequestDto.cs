using System.ComponentModel.DataAnnotations;

namespace MenuOnline.Dtos.Client;

public record ClientLoginRequestDto
(
  [Required] string Phone
);
