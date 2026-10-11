using System.ComponentModel.DataAnnotations;
using MenuOnline.Enums;

namespace MenuOnline.Models;

public class Address
{
  public int Id { get; set; }
  [Required, MaxLength(9)]
  public string Cep { get; set; } = string.Empty;
  [Required]
  public string City { get; set; } = string.Empty;
  public string Complement { get; set; } = string.Empty;
  [Required]
  public string Neighborhood { get; set; } = string.Empty;
  [Required]
  public string Number { get; set; } = string.Empty;
  public string ReferencePoint { get; set; } = string.Empty;
  public States State { get; set; }
  [Required]
  public string Street { get; set; } = string.Empty;

  // Um endereco pertence a um Client OU a um User.
  public int? ClientId { get; set; }
  public Client? Client { get; set; }

  public int? UserId { get; set; }
  public User? User { get; set; }
}
