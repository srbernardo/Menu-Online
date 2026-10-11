using MenuOnline.Data;
using MenuOnline.Dtos.Order;
using MenuOnline.Mappers;
using MenuOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Controllers
{
  [Route("api")]
  [ApiController]
  public class OrderController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly ILogger<OrderController> _logger;

    public OrderController(AppDbContext context, ILogger<OrderController> logger)
    {
      _context = context;
      _logger = logger;
    }

    private IQueryable<Order> OrdersWithRelations() =>
      _context.Orders
        .Include(o => o.Client)
        .Include(o => o.DeliveryAddress)
        .Include(o => o.ItemOrders).ThenInclude(io => io.Item);

    [HttpGet("orders")]
    public async Task<IActionResult> GetAllAsync(
      [FromQuery] int userId, [FromQuery] int? clientId, [FromQuery] Order.StatusOrder? status)
    {
      var query = OrdersWithRelations().AsNoTracking().Where(o => o.UserId == userId);

      if (clientId.HasValue)
        query = query.Where(o => o.ClientId == clientId.Value);

      if (status.HasValue)
        query = query.Where(o => o.Status == status.Value);

      var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
      return Ok(orders.Select(o => o.ToOrderDto()));
    }

    [HttpGet("order/{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
    {
      var order = await OrdersWithRelations().AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
      if (order == null)
        return NotFound();

      return Ok(order.ToOrderDto());
    }

    [HttpPost("order")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateOrderRequestDto dto)
    {
      if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId))
        return BadRequest("Usuário não encontrado!");

      if (!await _context.Clients.AnyAsync(c => c.Id == dto.ClientId))
        return BadRequest("Cliente não encontrado!");

      if (dto.DeliveryAddressId.HasValue &&
          !await _context.Addresses.AnyAsync(a => a.Id == dto.DeliveryAddressId && a.ClientId == dto.ClientId))
        return BadRequest("Endereço de entrega não pertence ao cliente!");

      // Mesmo item repetido na lista vira uma unica linha (chave composta ItemId+OrderId)
      var lines = dto.Items
        .GroupBy(i => i.ItemId)
        .Select(g => new { ItemId = g.Key, Quantity = g.Sum(x => x.Quantity) })
        .ToList();

      var itemIds = lines.Select(l => l.ItemId).ToList();
      var items = await _context.Items
        .AsNoTracking()
        .Where(i => itemIds.Contains(i.Id) && i.UserId == dto.UserId)
        .ToDictionaryAsync(i => i.Id);

      if (items.Count != itemIds.Count)
        return BadRequest("Um ou mais itens não foram encontrados para este usuário!");

      var order = new Order
      {
        UserId = dto.UserId,
        ClientId = dto.ClientId,
        DeliveryAddressId = dto.DeliveryAddressId,
        DeliveryFee = dto.DeliveryFee,
        Status = Order.StatusOrder.Pending
      };

      foreach (var line in lines)
      {
        order.ItemOrders.Add(new ItemOrder
        {
          ItemId = line.ItemId,
          Quantity = line.Quantity,
          UnitPrice = items[line.ItemId].Price
        });
      }

      order.TotalAmount = order.ItemOrders.Sum(io => io.UnitPrice * io.Quantity) + order.DeliveryFee;

      using var transaction = await _context.Database.BeginTransactionAsync();
      try
      {
        await order.SetNumberAsync(_context);
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
      }
      catch (Exception e)
      {
        await transaction.RollbackAsync();
        _logger.LogError(e, "Erro ao criar pedido");
        return StatusCode(500);
      }

      var created = await OrdersWithRelations().AsNoTracking().FirstAsync(o => o.Id == order.Id);
      return CreatedAtAction(nameof(GetByIdAsync), new { id = order.Id }, created.ToOrderDto());
    }

    // Status so avanca: Pending -> InPreparation -> Ready -> Delivered
    [HttpPatch("order/{id:int}/status")]
    public async Task<IActionResult> UpdateStatusAsync([FromRoute] int id, [FromBody] UpdateOrderStatusRequestDto dto)
    {
      var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
      if (order == null)
        return NotFound();

      if (!Enum.IsDefined(dto.Status))
        return BadRequest("Status inválido!");

      if (dto.Status <= order.Status)
        return BadRequest($"Não é possível mudar o status de {order.Status} para {dto.Status}.");

      order.Status = dto.Status;
      await _context.SaveChangesAsync();

      var updated = await OrdersWithRelations().AsNoTracking().FirstAsync(o => o.Id == id);
      return Ok(updated.ToOrderDto());
    }

    // Cancelamento: somente pedidos ainda pendentes.
    [HttpDelete("order/{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
      var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
      if (order == null)
        return NotFound();

      if (order.Status != Order.StatusOrder.Pending)
        return Conflict("Somente pedidos pendentes podem ser cancelados.");

      _context.Orders.Remove(order);
      await _context.SaveChangesAsync();
      return NoContent();
    }
  }
}
