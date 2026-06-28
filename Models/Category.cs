using System;
using System.ComponentModel.DataAnnotations;

namespace MenuOnline.Models;

public class Category
{
  public int Id { get; set; }
  [Required]
  public string Title { get; set; } = string.Empty;
  public int UserId { get; set; }
  public User? User { get; set; }
  public List<Item> Items { get; set; } = new List<Item>();
}
