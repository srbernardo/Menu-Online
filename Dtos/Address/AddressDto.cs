namespace MenuOnline.Dtos.Address;

public record AddressDto(
    int Id,
    string Cep,
    string Street,
    string Number,
    string Complement,
    string Neighborhood,
    string City,
    string State,
    string ReferencePoint,
    int UserId
);
