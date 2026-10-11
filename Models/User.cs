using System.ComponentModel.DataAnnotations;

namespace MenuOnline.Models;

public class User
{
  public enum StatusUser { Active, Inactive, Suspended }

  public enum RoleUser { Admin, Loja }

  public int Id { get; set; }
  [Required, EmailAddress]
  public string Email { get; set; } = string.Empty;
  public string PasswordHash { get; set; } = string.Empty;
  public RoleUser Role { get; set; }
  public string Description { get; set; } = string.Empty;
  [Required]
  public string Name { get; set; } = string.Empty;
  public string WhatsappNumber { get; set; } = string.Empty;
  public StatusUser Status { get; set; }

  public List<Order> Orders { get; set; } = new List<Order>();
  public List<Item> Items { get; set; } = new List<Item>();
  public List<RateTime> RateTimes { get; set; } = new List<RateTime>();

  // 1:1 - FK fica em Address.UserId / BusinessHours.UserId
  public Address? Address { get; set; }
  public BusinessHours? BusinessHours { get; set; }
}
