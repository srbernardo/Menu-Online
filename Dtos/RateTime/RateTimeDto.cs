namespace MenuOnline.Dtos.RateTime;

public record RateTimeDto(
    int Id,
    decimal Km,
    string TimeInMinutes,
    decimal Rate,
    int UserId
);
