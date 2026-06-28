namespace MenuOnline.Dtos.Category;

public record UpsertCategoryRequestDto(
    string Title,
    int UserId
);
