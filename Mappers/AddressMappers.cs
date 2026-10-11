using MenuOnline.Dtos.Address;
using MenuOnline.Models;

namespace MenuOnline.Mappers;

public static class AddressMappers
{
  public static AddressDto ToAddressDto(this Address model)
  {
    return new AddressDto
    (
      model.Id,
      model.Cep,
      model.City,
      model.Complement,
      model.Neighborhood,
      model.Number,
      model.ReferencePoint,
      model.State,
      model.Street
    );
  }

  public static Address ToAddressFromDto(this UpsertAddressRequestDto dto)
  {
    var address = new Address();
    dto.ApplyTo(address);
    return address;
  }

  public static void ApplyTo(this UpsertAddressRequestDto dto, Address address)
  {
    address.Cep = dto.Cep;
    address.City = dto.City;
    address.Complement = dto.Complement ?? "";
    address.Neighborhood = dto.Neighborhood;
    address.Number = dto.Number;
    address.ReferencePoint = dto.ReferencePoint ?? "";
    address.State = dto.State;
    address.Street = dto.Street;
  }
}
