using System;

namespace MenuOnline.Models;

public class BusinessHours
{
  public int Id { get; set; }
  public int UserId { get; set; }
  public User? User { get; set; }
  public string Monday { get; set; } = string.Empty;
  public string Tuesday { get; set; } = string.Empty;
  public string Wednesday { get; set; } = string.Empty;
  public string Thursday { get; set; } = string.Empty;
  public string Friday { get; set; } = string.Empty;
  public string Saturday { get; set; } = string.Empty;
  public string Sunday { get; set; } = string.Empty;
}
