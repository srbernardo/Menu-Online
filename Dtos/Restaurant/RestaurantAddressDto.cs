namespace MenuOnline.Dtos.Restaurant;

public record RestaurantAddressDto
(
  string Cep,
  string Street,
  string Number,
  string Complement,
  string Neighborhood,
  string City,
  string State,
  string ReferencePoint
);
