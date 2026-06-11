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
  public class AddressController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly ILogger<AddressController> _logger;
    public AddressController(AppDbContext context, ILogger<AddressController> logger)
    {
      _context = context;
      _logger = logger;

    }

    [HttpGet("addresses")]
    public async Task<IActionResult> GetAllAsync([FromBody] int userId)
    {
      // var addresses = await _context.Addresses
      //   .Where(a => a.UserId == userId)
      //   .AsNoTracking()
      //   .Select(a => a.ToAddressDto())
      //   .ToListAsync();

      return Ok(new List<string> { "Address1", "Address2" });
    }
  }
}
