namespace MenuOnline.Dtos.BusinessHours;

public record UpsertBusinessHoursRequestDto(
    int UserId,
    string Monday,
    string Tuesday,
    string Wednesday,
    string Thursday,
    string Friday,
    string Saturday,
    string Sunday
);
