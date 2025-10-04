using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using MenuOnline.Data;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Models;

public class Order
{
  public enum StatusOrder { Pending, InPreparation, Ready, Delivered }

  public int Id { get; set; }
  public DateTime OrderDate { get; set; } = DateTime.Now;
  public StatusOrder Status { get; set; }
  [Column(TypeName = "decimal(18,2)")]
  public decimal TotalAmount { get; set; }
  public int UserId { get; set; }
  public User? User { get; set; }
  public int ClientId { get; set; }
  public Client? Client { get; set; }
  public List<ItemOrder> ItemOrders { get; set; } = new List<ItemOrder>();
  [Column(TypeName = "decimal(18,2)")]
  public decimal DeliveryFee { get; set; }
  public int Number { get; private set; }
  public Address? DeliveryAddress { get; set; }
  public int? DeliveryAddressId { get; set; }

  [NotMapped]
  public List<Item> Items => ItemOrders?.Select(io => io.Item!).ToList() ?? new List<Item>();

  public async Task<int> SetNumberAsync(AppDbContext db, CancellationToken ct = default)
  {
    var start = DateTime.Today;
    var endInclusive = start.AddDays(1).AddTicks(-1);

    var last = await db.Orders
      .Where(o => o.UserId == this.UserId)
      .Where(o => o.OrderDate >= start && o.OrderDate < endInclusive)
      .MaxAsync(o => (int?)o.Number, ct) ?? 0;

    var next = last + 1;
    this.Number = next;

    return next;
  }
}
