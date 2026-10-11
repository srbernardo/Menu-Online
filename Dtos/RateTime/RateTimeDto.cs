namespace MenuOnline.Dtos.RateTime;

public record RateTimeDto
(
  int Id,
  string TimeInMinutes,
  decimal Km,
  decimal Rate
);
