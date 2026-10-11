using System.ComponentModel.DataAnnotations.Schema;

namespace MenuOnline.Models;

public class ItemOrder
{
  public int ItemId { get; set; }
  public Item? Item { get; set; }
  public int OrderId { get; set; }
  public Order? Order { get; set; }
  public int Quantity { get; set; } = 1;
  // Preco do item no momento da compra
  [Column(TypeName = "decimal(18,2)")]
  public decimal UnitPrice { get; set; }
}
