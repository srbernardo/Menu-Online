using System;
using System.ComponentModel.DataAnnotations;

namespace MenuOnline.Models;

public class Client
{
  public int Id { get; set; }
  [Required]
  public string Name { get; set; } = string.Empty;
  [Required]
  public string WhatsappNumber { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public List<Order> Orders { get; set; } = new List<Order>();
  public List<Address> Addresses { get; set; } = new List<Address>();
  public DateOnly BirthDate { get; set; }
}
