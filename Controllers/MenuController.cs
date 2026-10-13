using MenuOnline.Data;
using MenuOnline.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
  // Vitrine publica do cliente: um unico fetch retorna tudo (loja + cardapio).
  // Espelha o contrato em menu-online-web/src/types/menu.ts.
  [Route("api")]
  [ApiController]
  public class MenuController : ControllerBase
  {
    private readonly AppDbContext _context;

    public MenuController(AppDbContext context)
    {
      _context = context;
    }

    [HttpGet("menu/{slug}")]
    public async Task<IActionResult> GetMenuAsync([FromRoute] string slug)
    {
      var normalized = UserMappers.NormalizeSlug(slug);

      var user = await _context.Users
        .AsNoTracking()
        .Include(u => u.Address)
        .FirstOrDefaultAsync(u => u.Slug == normalized);

      if (user == null)
        return NotFound();

      // So categorias que tem item desta loja; so os itens desta loja.
      var categories = await _context.Categories
        .AsNoTracking()
        .Where(c => c.Items.Any(i => i.UserId == user.Id))
        .OrderBy(c => c.Title)
        .Include(c => c.Items.Where(i => i.UserId == user.Id))
          .ThenInclude(i => i.ItemExtras)
            .ThenInclude(ie => ie.Extra)
        .ToListAsync();

      // Taxa exibida na vitrine: a menor taxa da tabela da loja ("a partir de").
      // Sem tabela cadastrada, entrega gratis.
      var deliveryFee = await _context.RateTimes
        .AsNoTracking()
        .Where(r => r.UserId == user.Id)
        .MinAsync(r => (decimal?)r.Rate) ?? 0;

      return Ok(user.ToMenuDto(categories, deliveryFee));
    }
  }
}
