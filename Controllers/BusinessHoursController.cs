using MenuOnline.Data;
using MenuOnline.Dtos.BusinessHours;
using MenuOnline.Mappers;
using MenuOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
    [Route("api")]
    [ApiController]
    public class BusinessHoursController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BusinessHoursController> _logger;

        public BusinessHoursController(AppDbContext context, ILogger<BusinessHoursController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("business-hours/user/{userId:int}")]
        public async Task<IActionResult> GetByUserAsync(int userId)
        {
            var bh = await _context.BusinessHours
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (bh == null)
                return NotFound();

            return Ok(bh.ToBusinessHoursDto());
        }

        [HttpPost("business-hours")]
        public async Task<IActionResult> CreateAsync([FromBody] UpsertBusinessHoursRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Horário vazio!");

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user == null)
                return BadRequest("Usuário não encontrado!");

            try
            {
                var bh = dto.ToBusinessHoursFromCreateDto();
                await _context.BusinessHours.AddAsync(bh);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetByUserAsync), new { userId = bh.UserId }, bh.ToBusinessHoursDto());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro ao criar horário");
                return StatusCode(500);
            }
        }

        [HttpPut("business-hours/{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpsertBusinessHoursRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Horário vazio!");

            var bh = await _context.BusinessHours.FirstOrDefaultAsync(b => b.Id == id);
            if (bh == null)
                return NotFound();

            bh.UserId = dto.UserId;
            bh.Monday = dto.Monday;
            bh.Tuesday = dto.Tuesday;
            bh.Wednesday = dto.Wednesday;
            bh.Thursday = dto.Thursday;
            bh.Friday = dto.Friday;
            bh.Saturday = dto.Saturday;
            bh.Sunday = dto.Sunday;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(bh.ToBusinessHoursDto());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro ao atualizar horário");
                return StatusCode(500);
            }
        }

        [HttpDelete("business-hours/{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var bh = await _context.BusinessHours.FirstOrDefaultAsync(b => b.Id == id);
            if (bh == null)
                return NotFound();

            _context.BusinessHours.Remove(bh);
            await _context.SaveChangesAsync();

            return Ok(bh.ToBusinessHoursDto());
        }
    }
}
