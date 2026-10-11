using System.ComponentModel.DataAnnotations;
using MenuOnline.Enums;

namespace MenuOnline.Dtos.Address;

public record UpsertAddressRequestDto
(
  [Required, MaxLength(9)] string Cep,
  [Required] string City,
  string? Complement,
  [Required] string Neighborhood,
  [Required] string Number,
  string? ReferencePoint,
  States State,
  [Required] string Street
);
