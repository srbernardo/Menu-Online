using MenuOnline.Dtos.Address;
using MenuOnline.Models;

namespace MenuOnline.Mappers;

public static class AddressMappers
{
    public static AddressDto ToAddressDto(this Address address)
    {
        return new AddressDto(
            address.Id,
            address.cep,
            address.street,
            address.number,
            address.complement,
            address.neighborhood,
            address.city,
            address.state.ToString(),
            address.referencePoint,
            address.UserId
        );
    }

    public static Address ToAddressFromCreateDto(this UpsertAddressRequestDto dto)
    {
        return new Address
        {
            cep = dto.Cep,
            street = dto.Street,
            number = dto.Number,
            complement = dto.Complement,
            neighborhood = dto.Neighborhood,
            city = dto.City,
            state = dto.State,
            referencePoint = dto.ReferencePoint,
            UserId = dto.UserId
        };
    }
}
