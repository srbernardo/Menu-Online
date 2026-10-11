using MenuOnline.Data;
using MenuOnline.Dtos.Category;
using MenuOnline.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
  [Route("api")]
  [ApiController]
  public class CategoryController : ControllerBase
  {
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
      _context = context;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetAllAsync()
    {
      var categories = await _context.Categories.AsNoTracking().OrderBy(c => c.Title).ToListAsync();
      return Ok(categories.Select(c => c.ToCategoryDto()));
    }

    [HttpGet("category/{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
    {
      var category = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
      if (category == null)
        return NotFound();

      return Ok(category.ToCategoryDto());
    }

    [HttpPost("category")]
    public async Task<IActionResult> CreateAsync([FromBody] UpsertCategoryRequestDto dto)
    {
      var category = dto.ToCategoryFromDto();
      await _context.Categories.AddAsync(category);
      await _context.SaveChangesAsync();
      return CreatedAtAction(nameof(GetByIdAsync), new { id = category.Id }, category.ToCategoryDto());
    }

    [HttpPut("category/{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] UpsertCategoryRequestDto dto)
    {
      var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
      if (category == null)
        return NotFound();

      category.Title = dto.Title;
      await _context.SaveChangesAsync();
      return Ok(category.ToCategoryDto());
    }

    [HttpDelete("category/{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
      var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
      if (category == null)
        return NotFound();

      if (await _context.Items.AnyAsync(i => i.CategoryId == id))
        return Conflict("Categoria possui itens e não pode ser removida.");

      _context.Categories.Remove(category);
      await _context.SaveChangesAsync();
      return NoContent();
    }
  }
}
