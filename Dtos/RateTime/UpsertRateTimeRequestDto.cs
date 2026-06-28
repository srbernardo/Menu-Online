namespace MenuOnline.Dtos.RateTime;

public record UpsertRateTimeRequestDto(
    decimal Km,
    string TimeInMinutes,
    decimal Rate,
    int UserId
);
