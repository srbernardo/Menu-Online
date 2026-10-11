using MenuOnline.Dtos.Address;

namespace MenuOnline.Dtos.Client;

public record ClientDto
(
  int Id,
  string Name,
  string WhatsappNumber,
  string Email,
  DateOnly BirthDate,
  List<AddressDto> Addresses
);
