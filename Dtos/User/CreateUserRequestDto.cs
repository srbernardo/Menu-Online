using System.ComponentModel.DataAnnotations;
using MenuOnline.Dtos.Address;
using MenuOnline.Dtos.BusinessHours;

namespace MenuOnline.Dtos.User;

// Address e obrigatorio: um User nao pode ser criado sem endereco.
public record CreateUserRequestDto
(
  [Required] string Name,
  [Required, EmailAddress] string Email,
  [Required, MinLength(6)] string Password,
  string? Description,
  string? WhatsappNumber,
  MenuOnline.Models.User.RoleUser Role,
  [Required] UpsertAddressRequestDto Address,
  UpsertBusinessHoursRequestDto? BusinessHours
);
