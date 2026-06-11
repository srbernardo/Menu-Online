namespace MenuOnline.Dtos.Restaurant;

public record RestaurantProfileDto
(
  int Id,
  string Slug,
  string Name,
  string Description,
  string WhatsappNumber,
  RestaurantAddressDto Address,
  RestaurantBusinessHoursDto BusinessHours,
  List<RestaurantRateTimeDto> RateTimes,
  List<RestaurantItemDto> Items
);
