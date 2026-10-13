using MenuOnline.Dtos.Address;
using MenuOnline.Dtos.BusinessHours;
using MenuOnline.Models;

namespace MenuOnline.Dtos.User;

public record UserDto
(
  int Id,
  string Name,
  string Slug,
  string Email,
  string Description,
  string WhatsappNumber,
  string Phone,
  string CoverUrl,
  string LogoUrl,
  bool IsOpen,
  MenuOnline.Models.User.RoleUser Role,
  MenuOnline.Models.User.StatusUser Status,
  AddressDto? Address,
  BusinessHoursDto? BusinessHours
);
