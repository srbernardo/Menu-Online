using MenuOnline.Data;
using MenuOnline.Dtos.RateTime;
using MenuOnline.Mappers;
using MenuOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
    [Route("api")]
    [ApiController]
    public class RateTimeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RateTimeController> _logger;

        public RateTimeController(AppDbContext context, ILogger<RateTimeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("rate-times/user/{userId:int}")]
        public async Task<IActionResult> GetByUserAsync(int userId)
        {
            var rateTimes = await _context.RateTimes
                .Where(r => r.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            return Ok(rateTimes.Select(r => r.ToRateTimeDto()).ToList());
        }

        [HttpPost("rate-times")]
        public async Task<IActionResult> CreateAsync([FromBody] UpsertRateTimeRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Taxa vazia!");

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user == null)
                return BadRequest("Usuário não encontrado!");

            try
            {
                var rt = dto.ToRateTimeFromCreateDto();
                await _context.RateTimes.AddAsync(rt);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetByUserAsync), new { userId = rt.UserId }, rt.ToRateTimeDto());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro ao criar taxa");
                return StatusCode(500);
            }
        }

        [HttpPut("rate-times/{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpsertRateTimeRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Taxa vazia!");

            var rt = await _context.RateTimes.FirstOrDefaultAsync(r => r.Id == id);
            if (rt == null)
                return NotFound();

            rt.Km = dto.Km;
            rt.TimeInMinutes = dto.TimeInMinutes;
            rt.Rate = dto.Rate;
            rt.UserId = dto.UserId;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(rt.ToRateTimeDto());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro ao atualizar taxa");
                return StatusCode(500);
            }
        }

        [HttpDelete("rate-times/{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var rt = await _context.RateTimes.FirstOrDefaultAsync(r => r.Id == id);
            if (rt == null)
                return NotFound();

            _context.RateTimes.Remove(rt);
            await _context.SaveChangesAsync();

            return Ok(rt.ToRateTimeDto());
        }
    }
}
