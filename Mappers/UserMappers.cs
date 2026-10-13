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
      model.Slug,
      model.Email,
      model.Description,
      model.WhatsappNumber,
      model.Phone,
      model.CoverUrl,
      model.LogoUrl,
      model.IsOpen,
      model.Role,
      model.Status,
      model.Address?.ToAddressDto(),
      model.BusinessHours?.ToBusinessHoursDto()
    );
  }

  public static string NormalizeSlug(string slug) =>
    slug.Trim().ToLowerInvariant();
}
