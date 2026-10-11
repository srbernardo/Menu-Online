using MenuOnline.Dtos.Order;
using MenuOnline.Models;

namespace MenuOnline.Mappers;

public static class OrderMappers
{
  // Requer Client, DeliveryAddress e ItemOrders.Item carregados (Include).
  public static OrderDto ToOrderDto(this Order model)
  {
    return new OrderDto
    (
      model.Id,
      model.Number,
      model.OrderDate,
      model.Status,
      model.DeliveryFee,
      model.TotalAmount,
      model.UserId,
      model.ClientId,
      model.Client?.Name ?? "",
      model.DeliveryAddress?.ToAddressDto(),
      model.ItemOrders
        .Select(io => new OrderItemDto(io.ItemId, io.Item?.Title ?? "", io.Quantity, io.UnitPrice))
        .ToList()
    );
  }
}
