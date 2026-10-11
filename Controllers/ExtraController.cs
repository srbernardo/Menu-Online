using MenuOnline.Data;
using MenuOnline.Dtos.Extra;
using MenuOnline.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
  [Route("api")]
  [ApiController]
  public class ExtraController : ControllerBase
  {
    private readonly AppDbContext _context;

    public ExtraController(AppDbContext context)
    {
      _context = context;
    }

    [HttpGet("extras")]
    public async Task<IActionResult> GetAllAsync()
    {
      var extras = await _context.Extras.AsNoTracking().OrderBy(e => e.Title).ToListAsync();
      return Ok(extras.Select(e => e.ToExtraDto()));
    }

    [HttpGet("extra/{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
    {
      var extra = await _context.Extras.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
      if (extra == null)
        return NotFound();

      return Ok(extra.ToExtraDto());
    }

    [HttpPost("extra")]
    public async Task<IActionResult> CreateAsync([FromBody] UpsertExtraRequestDto dto)
    {
      var extra = dto.ToExtraFromDto();
      await _context.Extras.AddAsync(extra);
      await _context.SaveChangesAsync();
      return CreatedAtAction(nameof(GetByIdAsync), new { id = extra.Id }, extra.ToExtraDto());
    }

    [HttpPut("extra/{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] UpsertExtraRequestDto dto)
    {
      var extra = await _context.Extras.FirstOrDefaultAsync(e => e.Id == id);
      if (extra == null)
        return NotFound();

      extra.Title = dto.Title;
      extra.Description = dto.Description ?? "";
      extra.Value = dto.Value;
      await _context.SaveChangesAsync();
      return Ok(extra.ToExtraDto());
    }

    // Remove o extra e os vinculos com itens (cascade em ItemExtra).
    [HttpDelete("extra/{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
      var extra = await _context.Extras.FirstOrDefaultAsync(e => e.Id == id);
      if (extra == null)
        return NotFound();

      _context.Extras.Remove(extra);
      await _context.SaveChangesAsync();
      return NoContent();
    }
  }
}
