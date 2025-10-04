using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MenuOnline.Models;

public class RateTime
{
  public int Id { get; set; }
  public string TimeInMinutes { get; set; } = string.Empty;
  [Column(TypeName = "decimal(2,1)")]
  public decimal Km { get; set; }
  [Column(TypeName = "decimal(18,2)")]
  public decimal Rate { get; set; }
  public int UserId { get; set; }
  public User? User { get; set; }
}
