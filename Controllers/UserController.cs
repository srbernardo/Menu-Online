using MenuOnline.Data;
using MenuOnline.Dtos.User;
using MenuOnline.Mappers;
using MenuOnline.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
  [Route("api")]
  [ApiController]
  public class UserController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly ILogger<UserController> _logger;
    private static readonly PasswordHasher<User> _hasher = new();

    public UserController(AppDbContext context, ILogger<UserController> logger)
    {
      _context = context;
      _logger = logger;
    }

    private IQueryable<User> UsersWithRelations() =>
      _context.Users.Include(u => u.Address).Include(u => u.BusinessHours);

    [HttpGet("users")]
    public async Task<IActionResult> GetAllAsync()
    {
      var users = await UsersWithRelations().AsNoTracking().OrderBy(u => u.Name).ToListAsync();
      return Ok(users.Select(u => u.ToUserDto()));
    }

    [HttpGet("user/{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
    {
      var user = await UsersWithRelations().AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
      if (user == null)
        return NotFound();

      return Ok(user.ToUserDto());
    }

    // Um User nao pode ser criado sem endereco (Address e obrigatorio no DTO).
    [HttpPost("user")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUserRequestDto dto)
    {
      var email = dto.Email.Trim().ToLowerInvariant();
      if (await _context.Users.AnyAsync(u => u.Email == email))
        return Conflict("Já existe um usuário com esse e-mail.");

      var slug = MenuOnline.Mappers.UserMappers.NormalizeSlug(dto.Slug);
      if (await _context.Users.AnyAsync(u => u.Slug == slug))
        return Conflict("Já existe uma loja com esse slug.");

      var user = new User
      {
        Name = dto.Name,
        Slug = slug,
        Email = email,
        Description = dto.Description ?? "",
        WhatsappNumber = dto.WhatsappNumber ?? "",
        Phone = dto.Phone ?? "",
        CoverUrl = dto.CoverUrl ?? "",
        LogoUrl = dto.LogoUrl ?? "",
        Role = dto.Role,
        Status = Models.User.StatusUser.Active,
        Address = dto.Address.ToAddressFromDto(),
        BusinessHours = dto.BusinessHours?.ToBusinessHoursFromDto()
      };
      user.PasswordHash = _hasher.HashPassword(user, dto.Password);

      try
      {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
      }
      catch (Exception e)
      {
        _logger.LogError(e, "Erro ao criar usuário");
        return StatusCode(500);
      }

      return CreatedAtAction(nameof(GetByIdAsync), new { id = user.Id }, user.ToUserDto());
    }

    [HttpPut("user/{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] UpdateUserRequestDto dto)
    {
      var user = await UsersWithRelations().FirstOrDefaultAsync(u => u.Id == id);
      if (user == null)
        return NotFound();

      var email = dto.Email.Trim().ToLowerInvariant();
      if (await _context.Users.AnyAsync(u => u.Email == email && u.Id != id))
        return Conflict("Já existe um usuário com esse e-mail.");

      var slug = MenuOnline.Mappers.UserMappers.NormalizeSlug(dto.Slug);
      if (await _context.Users.AnyAsync(u => u.Slug == slug && u.Id != id))
        return Conflict("Já existe uma loja com esse slug.");

      user.Name = dto.Name;
      user.Slug = slug;
      user.Email = email;
      user.Description = dto.Description ?? "";
      user.WhatsappNumber = dto.WhatsappNumber ?? "";
      user.Phone = dto.Phone ?? "";
      user.CoverUrl = dto.CoverUrl ?? "";
      user.LogoUrl = dto.LogoUrl ?? "";
      user.Role = dto.Role;
      user.Status = dto.Status;

      if (user.Address == null)
        user.Address = dto.Address.ToAddressFromDto();
      else
        dto.Address.ApplyTo(user.Address);

      if (dto.BusinessHours != null)
      {
        if (user.BusinessHours == null)
          user.BusinessHours = dto.BusinessHours.ToBusinessHoursFromDto();
        else
          dto.BusinessHours.ApplyTo(user.BusinessHours);
      }

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (Exception e)
      {
        _logger.LogError(e, "Erro ao atualizar usuário");
        return StatusCode(500);
      }

      return Ok(user.ToUserDto());
    }

    // Toggle manual de aberto/fechado da loja.
    [HttpPatch("user/{id:int}/store-open")]
    public async Task<IActionResult> UpdateStoreOpenAsync([FromRoute] int id, [FromBody] UpdateStoreOpenRequestDto dto)
    {
      var user = await UsersWithRelations().FirstOrDefaultAsync(u => u.Id == id);
      if (user == null)
        return NotFound();

      user.IsOpen = dto.IsOpen;
      await _context.SaveChangesAsync();
      return Ok(user.ToUserDto());
    }

    [HttpDelete("user/{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
      var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
      if (user == null)
        return NotFound();

      if (await _context.Orders.AnyAsync(o => o.UserId == id))
        return Conflict("Usuário possui pedidos e não pode ser removido. Inative-o (Status).");

      _context.Users.Remove(user);
      await _context.SaveChangesAsync();
      return NoContent();
    }
  }
}
