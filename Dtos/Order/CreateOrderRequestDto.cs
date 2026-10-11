using System.ComponentModel.DataAnnotations;

namespace MenuOnline.Dtos.Order;

public record OrderItemRequestDto
(
  int ItemId,
  [Range(1, 1000)] int Quantity
);

public record CreateOrderRequestDto
(
  int UserId,
  int ClientId,
  int? DeliveryAddressId,
  [Range(0, 100000)] decimal DeliveryFee,
  [Required, MinLength(1)] List<OrderItemRequestDto> Items
);
