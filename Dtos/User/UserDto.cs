using MenuOnline.Dtos.Address;
using MenuOnline.Dtos.BusinessHours;
using MenuOnline.Models;

namespace MenuOnline.Dtos.User;

public record UserDto
(
  int Id,
  string Name,
  string Email,
  string Description,
  string WhatsappNumber,
  MenuOnline.Models.User.RoleUser Role,
  MenuOnline.Models.User.StatusUser Status,
  AddressDto? Address,
  BusinessHoursDto? BusinessHours
);
