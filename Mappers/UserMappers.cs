using MenuOnline.Dtos.User;
using MenuOnline.Models;

namespace MenuOnline.Mappers;

public static class UserMappers
{
  // Requer Address e BusinessHours carregados (Include).
  public static UserDto ToUserDto(this User model)
  {
    return new UserDto
    (
      model.Id,
      model.Name,
      model.Email,
      model.Description,
      model.WhatsappNumber,
      model.Role,
      model.Status,
      model.Address?.ToAddressDto(),
      model.BusinessHours?.ToBusinessHoursDto()
    );
  }
}
