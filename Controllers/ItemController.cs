using MenuOnline.Data;
using MenuOnline.Dtos.Item;
using MenuOnline.Mappers;
using MenuOnline.Models;
using MenuOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
  [Route("api")]
  [ApiController]
  public class ItemController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly ILogger<ItemController> _logger;
    private readonly MediaService _media;

    public ItemController(AppDbContext context, ILogger<ItemController> logger, MediaService media)
    {
      _context = context;
      _logger = logger;
      _media = media;
    }

    private IQueryable<Item> ItemsWithRelations() =>
      _context.Items
        .Include(i => i.Category)
        .Include(i => i.ItemExtras).ThenInclude(ie => ie.Extra);

    [HttpGet("items")]
    public async Task<IActionResult> GetAllAsync([FromQuery] int userId)
    {
      var items = await ItemsWithRelations()
        .AsNoTracking()
        .Where(i => i.UserId == userId)
        .ToListAsync();

      return Ok(items.Select(i => i.ToItemDto()));
    }

    [HttpGet("item/{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
    {
      var item = await ItemsWithRelations().AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
      if (item == null)
        return NotFound();

      return Ok(item.ToItemDto());
    }

    [HttpPost("item")]
    public async Task<IActionResult> CreateAsync([FromBody] UpsertItemRequestDto dto)
    {
      if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
        return BadRequest("Categoria não encontrada!");

      if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId))
        return BadRequest("Usuário não encontrado!");

      var extraIds = (dto.ExtraIds ?? new List<int>()).Distinct().ToList();
      var extras = await _context.Extras.Where(e => extraIds.Contains(e.Id)).ToListAsync();
      if (extras.Count != extraIds.Count)
        return BadRequest("Um ou mais extras não foram encontrados!");

      var newItem = dto.ToItemFromCreateDto();
      foreach (var extra in extras)
        newItem.ItemExtras.Add(new ItemExtra { Extra = extra });

      try
      {
        await _context.Items.AddAsync(newItem);
        await _context.SaveChangesAsync();
      }
      catch (Exception e)
      {
        _logger.LogError(e, "Erro ao criar item");
        return StatusCode(500);
      }

      var created = await ItemsWithRelations().AsNoTracking().FirstAsync(i => i.Id == newItem.Id);
      return CreatedAtAction(nameof(GetByIdAsync), new { id = newItem.Id }, created.ToItemDto());
    }

    [HttpPut("item/{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] UpsertItemRequestDto dto)
    {
      var itemDb = await _context.Items.Include(i => i.ItemExtras).FirstOrDefaultAsync(i => i.Id == id);
      if (itemDb == null)
        return NotFound();

      if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
        return BadRequest("Categoria não encontrada!");

      var extraIds = (dto.ExtraIds ?? new List<int>()).Distinct().ToList();
      var extras = await _context.Extras.Where(e => extraIds.Contains(e.Id)).ToListAsync();
      if (extras.Count != extraIds.Count)
        return BadRequest("Um ou mais extras não foram encontrados!");

      itemDb.CategoryId = dto.CategoryId;
      itemDb.Description = dto.Description ?? "";
      itemDb.ImageUrl = dto.ImageUrl ?? "";
      itemDb.Price = dto.Price;
      itemDb.Title = dto.Title;

      var toRemove = itemDb.ItemExtras.Where(ie => !extraIds.Contains(ie.ExtraId)).ToList();
      _context.ItemExtras.RemoveRange(toRemove);

      var existing = itemDb.ItemExtras.Select(ie => ie.ExtraId).ToHashSet();
      foreach (var extra in extras.Where(e => !existing.Contains(e.Id)))
        itemDb.ItemExtras.Add(new ItemExtra { ExtraId = extra.Id, ItemId = id });

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (Exception e)
      {
        _logger.LogError(e, "Erro ao atualizar item");
        return StatusCode(500);
      }

      var updated = await ItemsWithRelations().AsNoTracking().FirstAsync(i => i.Id == id);
      return Ok(updated.ToItemDto());
    }

    [HttpDelete("item/{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
      var itemDb = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);
      if (itemDb == null)
        return NotFound();

      if (await _context.ItemOrders.AnyAsync(io => io.ItemId == id))
        return Conflict("Item já foi vendido em pedidos e não pode ser removido.");

      await _media.PurgeOwnerAsync(MediaNames.Item, id);
      _context.Items.Remove(itemDb);
      await _context.SaveChangesAsync();
      return NoContent();
    }
  }
}
