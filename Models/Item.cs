using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MenuOnline.Models;

public class Item
{
  [Required]
  public Category? Category { get; set; }
  public int CategoryId { get; set; }
  public string Description { get; set; } = string.Empty;
  public List<ItemExtra> ItemExtras { get; set; } = new List<ItemExtra>();
  public int Id { get; set; }
  public string ImageUrl { get; set; } = string.Empty;
  public List<ItemOrder> ItemOrders { get; set; } = new List<ItemOrder>();
  [Required, Column(TypeName = "decimal(18,2)")]
  public decimal Price { get; set; }
  [Required]
  public string Title { get; set; } = string.Empty;
  public int UserId { get; set; }
  public User? User { get; set; }

  [NotMapped]
  public List<Extra> Extras => ItemExtras?.Select(ie => ie.Extra!).ToList() ?? new List<Extra>();
  [NotMapped]
  public List<Order> Orders => ItemOrders?.Select(io => io.Order!).ToList() ?? new List<Order>();
}
