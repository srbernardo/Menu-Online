namespace MenuOnline.Dtos.Extra;

public record UpsertExtraRequestDto(
    string Description,
    string Title,
    decimal Value,
    int UserId,
    int ItemId
);
