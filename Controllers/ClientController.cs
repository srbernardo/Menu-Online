using MenuOnline.Data;
using MenuOnline.Dtos.Address;
using MenuOnline.Dtos.Client;
using MenuOnline.Mappers;
using MenuOnline.Models;
using MenuOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
  [Route("api")]
  [ApiController]
  public class ClientController : ControllerBase
  {
    private readonly AppDbContext _context;

    public ClientController(AppDbContext context)
    {
      _context = context;
    }

    // ---------- Client ----------

    [HttpGet("clients")]
    public async Task<IActionResult> GetAllAsync()
    {
      var clients = await _context.Clients
        .Include(c => c.Addresses)
        .AsNoTracking()
        .OrderBy(c => c.Name)
        .ToListAsync();

      return Ok(clients.Select(c => c.ToClientDto()));
    }

    [HttpGet("client/{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
    {
      var client = await _context.Clients
        .Include(c => c.Addresses)
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == id);

      if (client == null)
        return NotFound();

      return Ok(client.ToClientDto());
    }

    [HttpPost("client")]
    public async Task<IActionResult> CreateAsync([FromBody] UpsertClientRequestDto dto)
    {
      var phone = PhoneNumber.Normalize(dto.WhatsappNumber);
      if (!PhoneNumber.IsValid(phone))
        return BadRequest("Número de telefone inválido!");

      if (await _context.Clients.AnyAsync(c => c.PhoneNormalized == phone))
        return Conflict("Já existe um cliente com este número de telefone!");

      var client = dto.ToClientFromDto();
      await _context.Clients.AddAsync(client);
      await _context.SaveChangesAsync();
      return CreatedAtAction(nameof(GetByIdAsync), new { id = client.Id }, client.ToClientDto());
    }

    [HttpPut("client/{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] UpsertClientRequestDto dto)
    {
      var client = await _context.Clients.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.Id == id);
      if (client == null)
        return NotFound();

      var phone = PhoneNumber.Normalize(dto.WhatsappNumber);
      if (!PhoneNumber.IsValid(phone))
        return BadRequest("Número de telefone inválido!");

      if (await _context.Clients.AnyAsync(c => c.PhoneNormalized == phone && c.Id != id))
        return Conflict("Já existe um cliente com este número de telefone!");

      dto.ApplyTo(client);
      await _context.SaveChangesAsync();
      return Ok(client.ToClientDto());
    }

    [HttpDelete("client/{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
      var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
      if (client == null)
        return NotFound();

      if (await _context.Orders.AnyAsync(o => o.ClientId == id))
        return Conflict("Cliente possui pedidos e não pode ser removido.");

      _context.Clients.Remove(client);
      await _context.SaveChangesAsync();
      return NoContent();
    }

    // ---------- Enderecos do cliente (varios por cliente) ----------

    [HttpGet("client/{clientId:int}/addresses")]
    public async Task<IActionResult> GetAddressesAsync([FromRoute] int clientId)
    {
      if (!await _context.Clients.AnyAsync(c => c.Id == clientId))
        return NotFound("Cliente não encontrado!");

      var addresses = await _context.Addresses
        .AsNoTracking()
        .Where(a => a.ClientId == clientId)
        .ToListAsync();

      return Ok(addresses.Select(a => a.ToAddressDto()));
    }

    [HttpGet("client/{clientId:int}/address/{addressId:int}")]
    public async Task<IActionResult> GetAddressAsync([FromRoute] int clientId, [FromRoute] int addressId)
    {
      var address = await _context.Addresses
        .AsNoTracking()
        .FirstOrDefaultAsync(a => a.Id == addressId && a.ClientId == clientId);

      if (address == null)
        return NotFound();

      return Ok(address.ToAddressDto());
    }

    [HttpPost("client/{clientId:int}/address")]
    public async Task<IActionResult> CreateAddressAsync([FromRoute] int clientId, [FromBody] UpsertAddressRequestDto dto)
    {
      if (!await _context.Clients.AnyAsync(c => c.Id == clientId))
        return NotFound("Cliente não encontrado!");

      var address = dto.ToAddressFromDto();
      address.ClientId = clientId;

      await _context.Addresses.AddAsync(address);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetAddressAsync), new { clientId, addressId = address.Id }, address.ToAddressDto());
    }

    [HttpPut("client/{clientId:int}/address/{addressId:int}")]
    public async Task<IActionResult> UpdateAddressAsync(
      [FromRoute] int clientId, [FromRoute] int addressId, [FromBody] UpsertAddressRequestDto dto)
    {
      var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.ClientId == clientId);
      if (address == null)
        return NotFound();

      dto.ApplyTo(address);
      await _context.SaveChangesAsync();
      return Ok(address.ToAddressDto());
    }

    [HttpDelete("client/{clientId:int}/address/{addressId:int}")]
    public async Task<IActionResult> DeleteAddressAsync([FromRoute] int clientId, [FromRoute] int addressId)
    {
      var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.ClientId == clientId);
      if (address == null)
        return NotFound();

      _context.Addresses.Remove(address);
      await _context.SaveChangesAsync();
      return NoContent();
    }
  }
}
