using MenuOnline.Dtos.Address;

namespace MenuOnline.Dtos.Order;

public record OrderItemDto
(
  int ItemId,
  string Title,
  int Quantity,
  decimal UnitPrice
);

public record OrderDto
(
  int Id,
  int Number,
  DateTime OrderDate,
  MenuOnline.Models.Order.StatusOrder Status,
  decimal DeliveryFee,
  decimal TotalAmount,
  int UserId,
  int ClientId,
  string ClientName,
  AddressDto? DeliveryAddress,
  List<OrderItemDto> Items
);
