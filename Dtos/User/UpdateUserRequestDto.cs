using System.ComponentModel.DataAnnotations;
using MenuOnline.Dtos.Address;
using MenuOnline.Dtos.BusinessHours;

namespace MenuOnline.Dtos.User;

public record UpdateUserRequestDto
(
  [Required] string Name,
  [Required, RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug deve ter apenas letras minúsculas, números e hífens.")] string Slug,
  [Required, EmailAddress] string Email,
  string? Description,
  string? WhatsappNumber,
  string? Phone,
  string? CoverUrl,
  string? LogoUrl,
  MenuOnline.Models.User.RoleUser Role,
  MenuOnline.Models.User.StatusUser Status,
  [Required] UpsertAddressRequestDto Address,
  UpsertBusinessHoursRequestDto? BusinessHours
);
