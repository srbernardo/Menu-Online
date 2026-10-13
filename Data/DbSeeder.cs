using MenuOnline.Enums;
using MenuOnline.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Data;

// Cria a loja de exemplo na primeira execucao em Development (idempotente).
public static class DbSeeder
{
  public static async Task SeedAsync(AppDbContext db)
  {
    if (await db.Users.AnyAsync())
      return;

    var img = (string seed, int w, int h) => $"https://picsum.photos/seed/{seed}/{w}/{h}";
    var hasher = new PasswordHasher<User>();

    var user = new User
    {
      Name = "Burger First",
      Slug = "burgerfirst",
      Email = "contato@burgerfirst.com",
      Description = "Hambúrgueres artesanais, batatas crocantes e milk-shakes cremosos. Pão e blend feitos no dia.",
      WhatsappNumber = "(11) 99999-0000",
      Phone = "(11) 3333-4444",
      CoverUrl = img("burgerfirst-cover", 1200, 500),
      LogoUrl = img("burgerfirst-logo", 300, 300),
      Role = User.RoleUser.Loja,
      Status = User.StatusUser.Active,
      IsOpen = true,
      Address = new Address
      {
        Cep = "01001-000",
        City = "São Paulo",
        Neighborhood = "Centro",
        Number = "120",
        State = States.SP,
        Street = "Rua das Flores"
      },
      BusinessHours = new BusinessHours
      {
        Monday = "11:00 - 23:00",
        Tuesday = "11:00 - 23:00",
        Wednesday = "11:00 - 23:00",
        Thursday = "11:00 - 23:00",
        Friday = "11:00 - 23:00",
        Saturday = "11:00 - 00:00",
        Sunday = "14:00 - 22:00"
      },
      RateTimes = new List<RateTime>
      {
        new() { TimeInMinutes = "30 min", Km = 2, Rate = 5 },
        new() { TimeInMinutes = "45 min", Km = 5, Rate = 8 },
        new() { TimeInMinutes = "60 min", Km = 10, Rate = 12 }
      }
    };
    user.PasswordHash = hasher.HashPassword(user, "loja123456");

    var lanches = new Category { Title = "Hambúrgueres" };
    var porcoes = new Category { Title = "Porções" };
    var bebidas = new Category { Title = "Bebidas" };
    var shakes = new Category { Title = "Milk-shakes" };
    var sobremesas = new Category { Title = "Sobremesas" };

    var bacon = new Extra { Title = "Bacon crocante", Description = "Fatias extras", Value = 4 };
    var cheddar = new Extra { Title = "Cheddar cremoso", Description = "Porção extra", Value = 3.5m };
    var ovo = new Extra { Title = "Ovo", Description = "Ovo frito", Value = 2.5m };

    Item Item(string title, string description, string? image, decimal price, Category category, params Extra[] extras) =>
      new()
      {
        Title = title,
        Description = description,
        ImageUrl = image ?? "",
        Price = price,
        Category = category,
        User = user,
        ItemExtras = extras.Select(e => new ItemExtra { Extra = e }).ToList()
      };

    var items = new List<Item>
    {
      Item("X-Burger", "Pão brioche, blend 160g, queijo prato, alface e tomate.", img("xburger", 400, 300), 28.9m, lanches, bacon, cheddar, ovo),
      Item("X-Bacon", "Blend 160g, muito bacon crocante, queijo cheddar e molho da casa.", img("xbacon", 400, 300), 33.9m, lanches, cheddar, ovo),
      Item("Duplo Smash", "Dois smash de 90g, queijo prato duplo, cebola caramelizada e picles.", img("smash", 400, 300), 36.5m, lanches, bacon, cheddar),
      Item("Veggie", "Hambúrguer de grão-de-bico, rúcula, tomate seco e maionese de ervas.", null, 31, lanches),
      Item("Batata frita", "Porção de 300g com sal e páprica.", img("fries", 400, 300), 18, porcoes, cheddar, bacon),
      Item("Onion rings", "Anéis de cebola empanados, 8 unidades.", img("onion", 400, 300), 21, porcoes),
      Item("Nuggets artesanais", "10 unidades com molho barbecue.", null, 24, porcoes),
      Item("Refrigerante lata", "Coca-Cola, Guaraná ou Sprite, 350ml.", img("soda", 400, 300), 6.5m, bebidas),
      Item("Suco natural", "Laranja, limão ou maracujá, 400ml.", img("juice", 400, 300), 10, bebidas),
      Item("Água mineral", "Com ou sem gás, 500ml.", null, 4, bebidas),
      Item("Chocolate belga", "Sorvete de baunilha, calda de chocolate belga e chantilly.", img("shake1", 400, 300), 22, shakes),
      Item("Morango", "Sorvete de creme com pedaços de morango.", img("shake2", 400, 300), 22, shakes),
      Item("Brownie com sorvete", "Brownie quentinho, bola de sorvete e calda.", img("brownie", 400, 300), 19.9m, sobremesas),
      Item("Torta de limão", "Fatia com merengue maçaricado.", null, 14, sobremesas)
    };

    await db.Users.AddAsync(user);
    await db.Categories.AddRangeAsync(lanches, porcoes, bebidas, shakes, sobremesas);
    await db.Extras.AddRangeAsync(bacon, cheddar, ovo);
    await db.Items.AddRangeAsync(items);
    await db.SaveChangesAsync();
  }
}
