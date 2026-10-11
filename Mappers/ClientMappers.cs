using MenuOnline.Dtos.Client;
using MenuOnline.Models;

namespace MenuOnline.Mappers;

public static class ClientMappers
{
  // Requer Addresses carregados (Include).
  public static ClientDto ToClientDto(this Client model)
  {
    return new ClientDto
    (
      model.Id,
      model.Name,
      model.WhatsappNumber,
      model.Email,
      model.BirthDate,
      model.Addresses.Select(a => a.ToAddressDto()).ToList()
    );
  }

  public static Client ToClientFromDto(this UpsertClientRequestDto dto)
  {
    var client = new Client();
    dto.ApplyTo(client);
    return client;
  }

  public static void ApplyTo(this UpsertClientRequestDto dto, Client client)
  {
    client.Name = dto.Name;
    client.WhatsappNumber = dto.WhatsappNumber;
    client.Email = dto.Email ?? "";
    client.BirthDate = dto.BirthDate;
  }
}
