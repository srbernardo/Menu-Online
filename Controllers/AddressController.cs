using MenuOnline.Data;
using MenuOnline.Dtos.Address;
using MenuOnline.Mappers;
using MenuOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
    [Route("api")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AddressController> _logger;

        public AddressController(AppDbContext context, ILogger<AddressController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("addresses/user/{userId:int}")]
        public async Task<IActionResult> GetByUserAsync(int userId)
        {
            var address = await _context.Addresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (address == null)
                return NotFound();

            return Ok(address.ToAddressDto());
        }

        [HttpPost("addresses")]
        public async Task<IActionResult> CreateAsync([FromBody] UpsertAddressRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Endereço vazio!");

            if (string.IsNullOrWhiteSpace(dto.Cep))
                return BadRequest("CEP é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Street))
                return BadRequest("Rua é obrigatória.");

            if (string.IsNullOrWhiteSpace(dto.Number))
                return BadRequest("Número é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Neighborhood))
                return BadRequest("Bairro é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.City))
                return BadRequest("Cidade é obrigatória.");

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user == null)
                return BadRequest("Usuário não encontrado!");

            try
            {
                var address = dto.ToAddressFromCreateDto();

                await _context.Addresses.AddAsync(address);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetByUserAsync), new { userId = address.UserId }, address.ToAddressDto());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro ao criar endereço");
                return StatusCode(500);
            }
        }

        [HttpPut("addresses/{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpsertAddressRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Endereço vazio!");

            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id);
            if (address == null)
                return NotFound();

            address.cep = dto.Cep;
            address.street = dto.Street;
            address.number = dto.Number;
            address.complement = dto.Complement;
            address.neighborhood = dto.Neighborhood;
            address.city = dto.City;
            address.state = dto.State;
            address.referencePoint = dto.ReferencePoint;
            address.UserId = dto.UserId;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(address.ToAddressDto());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro ao atualizar endereço");
                return StatusCode(500);
            }
        }

        [HttpDelete("addresses/{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id);
            if (address == null)
                return NotFound();

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return Ok(address.ToAddressDto());
        }
    }
}
