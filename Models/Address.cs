using System;
using System.ComponentModel.DataAnnotations;
using MenuOnline.Enums;

namespace MenuOnline.Models;

public class Address
{
  [Required, MaxLength(9)]
  public string cep { set; get; } = string.Empty;
  [Required]
  public string city { set; get; } = string.Empty;
  public string complement { set; get; } = string.Empty;
  public int Id { set; get; }
  [Required]
  public string neighborhood { set; get; } = string.Empty;
  [Required]
  public string number { set; get; } = string.Empty;
  public string referencePoint { set; get; } = string.Empty;
  [Required]
  public States state { set; get; }
  [Required]
  public string street { set; get; } = string.Empty;

  public int ClientId { get; set; }
  public Client? Client { get; set; }

  public int UserId { get; set; }
  public User? User { get; set; }
}
