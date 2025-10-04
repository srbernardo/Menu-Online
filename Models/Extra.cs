using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MenuOnline.Models;

public class Extra
{
  [Required]
  public string Description { get; set; } = string.Empty;
  public List<ItemExtra> ItemExtras { get; set; } = new List<ItemExtra>();
  public int Id { get; set; }
  [Required]
  public string Title { get; set; } = string.Empty;
  [Required, Column(TypeName = "decimal(18,2)")]
  public decimal Value { get; set; }

  [NotMapped]
  public List<Item> Items => ItemExtras?.Select(ie => ie.Item!).ToList() ?? new List<Item>();
}
