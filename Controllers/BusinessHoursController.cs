using MenuOnline.Data;
using MenuOnline.Dtos.BusinessHours;
using MenuOnline.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
  [Route("api")]
  [ApiController]
  public class BusinessHoursController : ControllerBase
  {
    private readonly AppDbContext _context;

    public BusinessHoursController(AppDbContext context)
    {
      _context = context;
    }

    [HttpGet("user/{userId:int}/business-hours")]
    public async Task<IActionResult> GetAsync([FromRoute] int userId)
    {
      var hours = await _context.BusinessHours.AsNoTracking().FirstOrDefaultAsync(b => b.UserId == userId);
      if (hours == null)
        return NotFound();

      return Ok(hours.ToBusinessHoursDto());
    }

    // Cria ou atualiza o horario de funcionamento (um por usuario).
    [HttpPut("user/{userId:int}/business-hours")]
    public async Task<IActionResult> UpsertAsync([FromRoute] int userId, [FromBody] UpsertBusinessHoursRequestDto dto)
    {
      if (!await _context.Users.AnyAsync(u => u.Id == userId))
        return NotFound("Usuário não encontrado!");

      var hours = await _context.BusinessHours.FirstOrDefaultAsync(b => b.UserId == userId);
      if (hours == null)
      {
        hours = dto.ToBusinessHoursFromDto();
        hours.UserId = userId;
        await _context.BusinessHours.AddAsync(hours);
      }
      else
      {
        dto.ApplyTo(hours);
      }

      await _context.SaveChangesAsync();
      return Ok(hours.ToBusinessHoursDto());
    }

    [HttpDelete("user/{userId:int}/business-hours")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int userId)
    {
      var hours = await _context.BusinessHours.FirstOrDefaultAsync(b => b.UserId == userId);
      if (hours == null)
        return NotFound();

      _context.BusinessHours.Remove(hours);
      await _context.SaveChangesAsync();
      return NoContent();
    }
  }
}
