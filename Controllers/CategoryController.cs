using MenuOnline.Data;
using MenuOnline.Dtos.Category;
using MenuOnline.Mappers;
using MenuOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
    [Route("api")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(AppDbContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("categories/user/{userId:int}")]
        public async Task<IActionResult> GetAllAsync(int userId)
        {
            var categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            return Ok(categories.Select(c => c.ToCategoryDto()).ToList());
        }

        [HttpPost("categories")]
        public async Task<IActionResult> CreateAsync([FromBody] UpsertCategoryRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Categoria vazia!");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Título é obrigatório.");

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user == null)
                return BadRequest("Usuário não encontrado!");

            try
            {
                var category = dto.ToCategoryFromCreateDto();
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();

                return Created("", category.ToCategoryDto());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro ao criar categoria");
                return StatusCode(500);
            }
        }

        [HttpPut("categories/{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpsertCategoryRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Categoria vazia!");

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return NotFound();

            category.Title = dto.Title;
            category.UserId = dto.UserId;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(category.ToCategoryDto());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro ao atualizar categoria");
                return StatusCode(500);
            }
        }

        [HttpDelete("categories/{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(category.ToCategoryDto());
        }
    }
}
