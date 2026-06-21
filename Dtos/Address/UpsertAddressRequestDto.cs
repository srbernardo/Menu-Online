using MenuOnline.Enums;

namespace MenuOnline.Dtos.Address;

public record UpsertAddressRequestDto(
    string Cep,
    string Street,
    string Number,
    string Complement,
    string Neighborhood,
    string City,
    States State,
    string ReferencePoint,
    int UserId
);
