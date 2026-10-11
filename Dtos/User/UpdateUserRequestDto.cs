using System.ComponentModel.DataAnnotations;
using MenuOnline.Dtos.Address;
using MenuOnline.Dtos.BusinessHours;

namespace MenuOnline.Dtos.User;

public record UpdateUserRequestDto
(
  [Required] string Name,
  [Required, EmailAddress] string Email,
  string? Description,
  string? WhatsappNumber,
  MenuOnline.Models.User.RoleUser Role,
  MenuOnline.Models.User.StatusUser Status,
  [Required] UpsertAddressRequestDto Address,
  UpsertBusinessHoursRequestDto? BusinessHours
);
