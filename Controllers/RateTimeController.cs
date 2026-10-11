using MenuOnline.Data;
using MenuOnline.Dtos.RateTime;
using MenuOnline.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
  [Route("api")]
  [ApiController]
  public class RateTimeController : ControllerBase
  {
    private readonly AppDbContext _context;

    public RateTimeController(AppDbContext context)
    {
      _context = context;
    }

    [HttpGet("user/{userId:int}/rate-times")]
    public async Task<IActionResult> GetAllAsync([FromRoute] int userId)
    {
      var rates = await _context.RateTimes
        .AsNoTracking()
        .Where(r => r.UserId == userId)
        .OrderBy(r => r.Km)
        .ToListAsync();

      return Ok(rates.Select(r => r.ToRateTimeDto()));
    }

    [HttpGet("rate-time/{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
    {
      var rate = await _context.RateTimes.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
      if (rate == null)
        return NotFound();

      return Ok(rate.ToRateTimeDto());
    }

    [HttpPost("user/{userId:int}/rate-time")]
    public async Task<IActionResult> CreateAsync([FromRoute] int userId, [FromBody] UpsertRateTimeRequestDto dto)
    {
      if (!await _context.Users.AnyAsync(u => u.Id == userId))
        return NotFound("Usuário não encontrado!");

      var rate = dto.ToRateTimeFromDto(userId);
      await _context.RateTimes.AddAsync(rate);
      await _context.SaveChangesAsync();
      return CreatedAtAction(nameof(GetByIdAsync), new { id = rate.Id }, rate.ToRateTimeDto());
    }

    [HttpPut("rate-time/{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] UpsertRateTimeRequestDto dto)
    {
      var rate = await _context.RateTimes.FirstOrDefaultAsync(r => r.Id == id);
      if (rate == null)
        return NotFound();

      rate.TimeInMinutes = dto.TimeInMinutes;
      rate.Km = dto.Km;
      rate.Rate = dto.Rate;
      await _context.SaveChangesAsync();
      return Ok(rate.ToRateTimeDto());
    }

    [HttpDelete("rate-time/{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
      var rate = await _context.RateTimes.FirstOrDefaultAsync(r => r.Id == id);
      if (rate == null)
        return NotFound();

      _context.RateTimes.Remove(rate);
      await _context.SaveChangesAsync();
      return NoContent();
    }
  }
}
