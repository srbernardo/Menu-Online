using MenuOnline.Data;
using MenuOnline.Mappers;
using MenuOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
  [Route("api/restaurants")]
  [ApiController]
  public class RestaurantController : ControllerBase
  {
    private readonly AppDbContext _context;

    public RestaurantController(AppDbContext context)
    {
      _context = context;
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug([FromRoute] string slug)
    {
      if (string.IsNullOrWhiteSpace(slug))
        return BadRequest("Slug é obrigatório.");

      var user = await _context.Users
        .AsNoTracking()
        .Include(u => u.Address)
        .Include(u => u.BusinessHours)
        .Include(u => u.RateTimes)
        .Include(u => u.Items).ThenInclude(i => i.Category)
        .Include(u => u.Items).ThenInclude(i => i.ItemExtras).ThenInclude(ie => ie.Extra)
        .FirstOrDefaultAsync(u =>
          u.Slug == slug.ToLowerInvariant() &&
          u.Status == MenuOnline.Models.User.StatusUser.Active);

      if (user == null)
        return NotFound("Restaurante não encontrado.");

      return Ok(user.ToRestaurantProfileDto());
    }
  }
}
