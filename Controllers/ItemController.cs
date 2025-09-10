using System.Linq.Expressions;
using MenuOnline.Data;
using MenuOnline.Dtos.Item;
using MenuOnline.Mappers;
using MenuOnline.Models;
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
    public ItemController(AppDbContext context, ILogger<ItemController> logger)
    {
      _context = context;
      _logger = logger;

    }

    [HttpGet("items")]
    public async Task<IActionResult> GetAllAsync([FromBody] int userId)
    {
      var items = await _context.Items
        .Where(i => i.UserId == userId)
        .AsNoTracking()
        .Select(i => i.ToItemDto(_context))
        .ToListAsync();
    
      return Ok(items);
    }

    [HttpPost("item")]
    public async Task<IActionResult> CreateAsync([FromBody] UpsetItemRequestDto itemRequestDto)
    {
      using var transaction = await _context.Database.BeginTransactionAsync();

      try
      {
        if (itemRequestDto == null)
          return BadRequest("Item vazio!");

        var category = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == itemRequestDto.CategoryId);
        if (category == null)
          return BadRequest("Categoria não encontrada!");

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == itemRequestDto.UserId);
        if (user == null)
          return BadRequest("Usuário não encontrado!");

        var extras = await _context.Extras.Where(e => itemRequestDto.ExtraIds.Contains(e.Id)).ToListAsync();
        if (extras.Count != itemRequestDto.ExtraIds.Count)
          return BadRequest("Um ou mais extras não foram encontrados!");

        var newItem = itemRequestDto.ToItemFromCreateDto();

        await _context.Items.AddAsync(newItem);
        await _context.SaveChangesAsync();
        
        if (extras.Count != 0)
        {
          var itemExtras = await _context.ItemExtras
            .Where(ie => ie.ItemId == newItem.Id)
            .ToListAsync();

          _context.ItemExtras.RemoveRange(itemExtras);
          await _context.SaveChangesAsync();
        }

        foreach (var extra in extras)
        {
          ItemExtra itemExtra = new ItemExtra
          {
            ExtraId = extra.Id,
            ItemId = newItem.Id
          };
          await _context.ItemExtras.AddAsync(itemExtra);
          await _context.SaveChangesAsync();
        }

        await transaction.CommitAsync();
        return Created("Item criado com sucesso!", newItem);
      }
      catch (Exception e)
      {
        await transaction.RollbackAsync();
        _logger.LogError(e, "Erro ao criar item");
        return StatusCode(500);
      }     
    }

    [HttpPut("item/{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] UpsetItemRequestDto item)
    {
      using var transaction = await _context.Database.BeginTransactionAsync();

      try
      {

        if (item == null)
          return BadRequest("Item vazio!");

        var itemDb = await _context.Items.Include(i => i.Extras).FirstOrDefaultAsync(i => i.Id == id);
        if (itemDb == null)
          return NotFound();

        var extras = await _context.Extras.Where(e => item.ExtraIds.Contains(e.Id)).ToListAsync();
        if (extras.Count != item.ExtraIds.Count)
          return BadRequest("Um ou mais extras não foram encontrados!");

        itemDb.CategoryId = item.CategoryId;
        itemDb.Description = item.Description;
        itemDb.ImageUrl = item.ImageUrl;
        itemDb.Price = item.Price;
        itemDb.Title = item.Title;

        itemDb.ItemExtras.Clear();

        foreach (var extra in extras)
        {
          ItemExtra itemExtra = new ItemExtra
          {
            ExtraId = extra.Id,
            ItemId = id
          };
          await _context.ItemExtras.AddAsync(itemExtra);
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(item);
      }
      catch (Exception e)
      {
        await transaction.RollbackAsync();
        _logger.LogError(e, "Erro ao atualizar item");
        return StatusCode(500);
      }
    }

    [HttpDelete("item/{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)  
    {
      var itemDb = await _context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
      if (itemDb == null)
        return NotFound();

      _context.Items.Remove(itemDb);
      await _context.SaveChangesAsync();
      return Ok(itemDb);
    } 
  }
}
