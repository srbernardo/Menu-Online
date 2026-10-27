using System.Security.Claims;
using MenuOnline.Data;
using MenuOnline.Dtos.Client;
using MenuOnline.Mappers;
using MenuOnline.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
  // Login do cliente da vitrine: somente telefone (sem senha) + cookie HttpOnly.
  [Route("api/auth/client")]
  [ApiController]
  public class ClientAuthController : ControllerBase
  {
    private readonly AppDbContext _context;

    public ClientAuthController(AppDbContext context)
    {
      _context = context;
    }

    // Check em tempo real enquanto o front digita o telefone.
    [HttpGet("lookup")]
    public async Task<IActionResult> LookupAsync([FromQuery] string? phone)
    {
      var normalized = PhoneNumber.Normalize(phone);

      if (!PhoneNumber.IsValid(normalized))
        return Ok(new ClientLookupResponseDto(false, null));

      var client = await _context.Clients
        .AsNoTracking()
        .Where(c => c.PhoneNormalized == normalized)
        .Select(c => new { c.Name })
        .FirstOrDefaultAsync();

      return Ok(new ClientLookupResponseDto(client != null, client?.Name));
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] ClientLoginRequestDto dto)
    {
      var normalized = PhoneNumber.Normalize(dto.Phone);
      if (!PhoneNumber.IsValid(normalized))
        return BadRequest("Número de telefone inválido!");

      var client = await _context.Clients
        .AsNoTracking()
        .Include(c => c.Addresses)
        .FirstOrDefaultAsync(c => c.PhoneNormalized == normalized);

      if (client == null)
        return NotFound("Número de telefone não cadastrado!");

      var claims = new List<Claim>
      {
        new(ClaimTypes.NameIdentifier, client.Id.ToString()),
        new(ClaimTypes.Name, client.Name),
        new("phone", client.WhatsappNumber)
      };

      var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
      var principal = new ClaimsPrincipal(identity);

      await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        new AuthenticationProperties
        {
          IsPersistent = true,
          ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
        });

      return Ok(client.ToClientDto());
    }

    // Dados do cliente logado (o cookie e renovado pelo SlidingExpiration).
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> MeAsync()
    {
      if (!TryGetClientId(out var clientId))
        return Unauthorized();

      var client = await _context.Clients
        .AsNoTracking()
        .Include(c => c.Addresses)
        .FirstOrDefaultAsync(c => c.Id == clientId);

      if (client == null)
      {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Unauthorized();
      }

      return Ok(client.ToClientDto());
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
      await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
      return NoContent();
    }

    private bool TryGetClientId(out int clientId)
    {
      return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out clientId);
    }
  }
}
