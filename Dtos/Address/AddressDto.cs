using MenuOnline.Enums;

namespace MenuOnline.Dtos.Address;

public record AddressDto
(
  int Id,
  string Cep,
  string City,
  string Complement,
  string Neighborhood,
  string Number,
  string ReferencePoint,
  States State,
  string Street
);
