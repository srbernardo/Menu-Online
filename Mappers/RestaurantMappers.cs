using MenuOnline.Dtos.Restaurant;
using MenuOnline.Models;

namespace MenuOnline.Mappers;

public static class RestaurantMappers
{
  public static RestaurantProfileDto ToRestaurantProfileDto(this User user)
  {
    var items = user.Items
      .Select(i => new RestaurantItemDto(
        i.Id,
        i.Title,
        i.Description,
        i.ImageUrl,
        i.Price,
        i.CategoryId,
        i.Category?.Title ?? string.Empty,
        i.Extras.Select(e => e.ToExtraDto()).ToList()))
      .ToList();

    var address = user.Address == null
      ? null!
      : new RestaurantAddressDto(
        user.Address.cep,
        user.Address.street,
        user.Address.number,
        user.Address.complement,
        user.Address.neighborhood,
        user.Address.city,
        user.Address.state.ToString(),
        user.Address.referencePoint);

    var businessHours = user.BusinessHours == null
      ? null!
      : new RestaurantBusinessHoursDto(
        user.BusinessHours.Monday,
        user.BusinessHours.Tuesday,
        user.BusinessHours.Wednesday,
        user.BusinessHours.Thursday,
        user.BusinessHours.Friday,
        user.BusinessHours.Saturday,
        user.BusinessHours.Sunday);

    var rateTimes = user.RateTimes
      .Select(r => new RestaurantRateTimeDto(r.Id, r.Km, r.TimeInMinutes, r.Rate))
      .ToList();

    return new RestaurantProfileDto(
      user.Id,
      user.Slug,
      user.Name,
      user.Description,
      user.WhatsappNumber,
      address,
      businessHours,
      rateTimes,
      items);
  }
}
