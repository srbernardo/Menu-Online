namespace MenuOnline.Dtos.Restaurant;

public record RestaurantRateTimeDto
(
  int Id,
  decimal Km,
  string TimeInMinutes,
  decimal Rate
);
