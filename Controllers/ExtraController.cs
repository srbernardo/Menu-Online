using MenuOnline.Data;
using MenuOnline.Dtos.Extra;
using MenuOnline.Mappers;
using MenuOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
    [Route("api")]
    [ApiController]
    public class ExtraController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ExtraController> _logger;

        public ExtraController(AppDbContext context, ILogger<ExtraController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("extras/item/{itemId:int}")]
        public async Task<IActionResult> GetAllAsync(int itemId)
        {
            var extras = await _context.ItemExtras
                .Where(ie => ie.ItemId == itemId)
                .Include(ie => ie.Extra)
                .Select(ie => ie.Extra!.ToExtraDto())
                .ToListAsync();

            return Ok(extras);
        }

        [HttpPost("extras")]
        public async Task<IActionResult> CreateAsync([FromBody] UpsertExtraRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Extra vazio!");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Título é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest("Descrição é obrigatória.");

            if (dto.Value <= 0)
                return BadRequest("Valor deve ser maior que zero.");

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user == null)
                return BadRequest("Usuário não encontrado!");

            var item = await _context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == dto.ItemId);
            if (item == null)
                return BadRequest("Item não encontrado!");

            try
            {
                var extra = dto.ToExtraFromCreateDto();
                await _context.Extras.AddAsync(extra);
                await _context.SaveChangesAsync();

                var itemExtra = new ItemExtra { ItemId = dto.ItemId, ExtraId = extra.Id };
                await _context.ItemExtras.AddAsync(itemExtra);
                await _context.SaveChangesAsync();

                return Created("", extra.ToExtraDto());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro ao criar extra");
                return StatusCode(500);
            }
        }

        [HttpPut("extras/{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpsertExtraRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Extra vazio!");

            var extra = await _context.Extras.FirstOrDefaultAsync(e => e.Id == id);
            if (extra == null)
                return NotFound();

            extra.Title = dto.Title;
            extra.Description = dto.Description;
            extra.Value = dto.Value;
            extra.UserId = dto.UserId;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(extra.ToExtraDto());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro ao atualizar extra");
                return StatusCode(500);
            }
        }

        [HttpDelete("extras/{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var extra = await _context.Extras.FirstOrDefaultAsync(e => e.Id == id);
            if (extra == null)
                return NotFound();

            _context.Extras.Remove(extra);
            await _context.SaveChangesAsync();

            return Ok(extra.ToExtraDto());
        }
    }
}
