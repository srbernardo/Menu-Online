using System.ComponentModel.DataAnnotations;
using MenuOnline.Dtos.Address;
using MenuOnline.Dtos.BusinessHours;

namespace MenuOnline.Dtos.User;

// Address e obrigatorio: um User nao pode ser criado sem endereco.
public record CreateUserRequestDto
(
  [Required] string Name,
  [Required, RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug deve ter apenas letras minúsculas, números e hífens.")] string Slug,
  [Required, EmailAddress] string Email,
  [Required, MinLength(6)] string Password,
  string? Description,
  string? WhatsappNumber,
  string? Phone,
  string? CoverUrl,
  string? LogoUrl,
  MenuOnline.Models.User.RoleUser Role,
  [Required] UpsertAddressRequestDto Address,
  UpsertBusinessHoursRequestDto? BusinessHours
);
