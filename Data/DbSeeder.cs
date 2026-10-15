using MenuOnline.Enums;
using MenuOnline.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Data;

// Cria a loja de exemplo na primeira execucao em Development (idempotente).
// Imagens: arquivos versionados em Assets/seed, servidos em /uploads/seed.
public static class DbSeeder
{
  private static readonly string[] Keys =
    ["cover", "logo", "xburger", "xbacon", "smash", "veggie", "fries",
     "onion", "nuggets", "soda", "juice", "water", "shake1", "shake2", "brownie", "torta"];

  // Trechos que identificam as URLs remotas antigas do seed (picsum, wikimedia, unsplash).
  private static readonly Dictionary<string, string[]> RemoteMarkers = new()
  {
    ["cover"] = ["burgerfirst-cover", "Double_Smash_Burger_with_Fries", "1761315414039-3955a6252f58"],
    ["logo"] = ["burgerfirst-logo", "Cheeseburger.jpg", "1568901346375-23c9450c58cd"],
    ["xburger"] = ["/seed/xburger/", "NCI_Visuals_Food_Hamburger", "1586190848861-99aa4a171e90"],
    ["xbacon"] = ["/seed/xbacon/", "Double_Bacon_Cheeseburger", "1700835880280-a0184101b9c8"],
    ["smash"] = ["/seed/smash/", "Smash_burger_-_Cambridge", "1607013251379-e6eecfffe234"],
    ["veggie"] = ["ultimate_veggie_burger", "1520072959219-c595dc870360"],
    ["fries"] = ["/seed/fries/", "Cajun_fries", "1630384060421-cb20d0e0649d"],
    ["onion"] = ["/seed/onion/", "Onion_rings", "1766589152292-3c052f0d87aa"],
    ["nuggets"] = ["Chicken_nuggets_on_a_plate", "1605291581926-df4bf7ee3e89"],
    ["soda"] = ["/seed/soda/", "Beverage_cans", "1622483767028-3f66f32aef97"],
    ["juice"] = ["/seed/juice/", "Orange_juice_1_edit1", "1613478223719-2ab802602423"],
    ["water"] = ["Bottle_of_Water", "1523362628745-0c100150b504"],
    ["shake1"] = ["/seed/shake1/", "Chocolate_milkshake", "1572490122747-3968b75cc699"],
    ["shake2"] = ["/seed/shake2/", "Strawberry_milk_shake", "1579954115545-a95591f28bfc"],
    ["brownie"] = ["/seed/brownie/", "Chocolate_brownies_without_table", "1606313564200-e75d5e30476c"],
    ["torta"] = ["Lemon_Meringue_Pie", "1785695797215-4345de8ab06c"]
  };

  private static string? LocalKeyForUrl(string? url)
  {
    if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
      return null;
    foreach (var (key, markers) in RemoteMarkers)
      if (markers.Any(url.Contains))
        return key;
    return null;
  }

  public static async Task SeedAsync(AppDbContext db, string publicBaseUrl)
  {
    var img = Keys.ToDictionary(k => k, k => $"{publicBaseUrl.TrimEnd('/')}/uploads/seed/{k}.jpg");

    if (!await db.Users.AnyAsync())
    {
    var hasher = new PasswordHasher<User>();

    var user = new User
    {
      Name = "Burger First",
      Slug = "burgerfirst",
      Email = "contato@burgerfirst.com",
      Description = "Hambúrgueres artesanais, batatas crocantes e milk-shakes cremosos. Pão e blend feitos no dia.",
      WhatsappNumber = "(11) 99999-0000",
      Phone = "(11) 3333-4444",
      CoverUrl = img["cover"],
      LogoUrl = img["logo"],
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
      Item("X-Burger", "Pão brioche, blend 160g, queijo prato, alface e tomate.", img["xburger"], 28.9m, lanches, bacon, cheddar, ovo),
      Item("X-Bacon", "Blend 160g, muito bacon crocante, queijo cheddar e molho da casa.", img["xbacon"], 33.9m, lanches, cheddar, ovo),
      Item("Duplo Smash", "Dois smash de 90g, queijo prato duplo, cebola caramelizada e picles.", img["smash"], 36.5m, lanches, bacon, cheddar),
      Item("Veggie", "Hambúrguer de grão-de-bico, rúcula, tomate seco e maionese de ervas.", img["veggie"], 31, lanches),
      Item("Batata frita", "Porção de 300g com sal e páprica.", img["fries"], 18, porcoes, cheddar, bacon),
      Item("Onion rings", "Anéis de cebola empanados, 8 unidades.", img["onion"], 21, porcoes),
      Item("Nuggets artesanais", "10 unidades com molho barbecue.", img["nuggets"], 24, porcoes),
      Item("Refrigerante lata", "Coca-Cola, Guaraná ou Sprite, 350ml.", img["soda"], 6.5m, bebidas),
      Item("Suco natural", "Laranja, limão ou maracujá, 400ml.", img["juice"], 10, bebidas),
      Item("Água mineral", "Com ou sem gás, 500ml.", img["water"], 4, bebidas),
      Item("Chocolate belga", "Sorvete de baunilha, calda de chocolate belga e chantilly.", img["shake1"], 22, shakes),
      Item("Morango", "Sorvete de creme com pedaços de morango.", img["shake2"], 22, shakes),
      Item("Brownie com sorvete", "Brownie quentinho, bola de sorvete e calda.", img["brownie"], 19.9m, sobremesas),
      Item("Torta de limão", "Fatia com merengue maçaricado.", img["torta"], 14, sobremesas)
    };

    await db.Users.AddAsync(user);
    await db.Categories.AddRangeAsync(lanches, porcoes, bebidas, shakes, sobremesas);
    await db.Extras.AddRangeAsync(bacon, cheddar, ovo);
    await db.Items.AddRangeAsync(items);
    await db.SaveChangesAsync();
    return;
    }

    // Banco já populado: converte apenas URLs remotas conhecidas do seed para locais.
    var dirty = false;

    foreach (var u in await db.Users.ToListAsync())
    {
      var coverKey = LocalKeyForUrl(u.CoverUrl);
      if (coverKey != null && u.CoverUrl != img[coverKey]) { u.CoverUrl = img[coverKey]; dirty = true; }
      var logoKey = LocalKeyForUrl(u.LogoUrl);
      if (logoKey != null && u.LogoUrl != img[logoKey]) { u.LogoUrl = img[logoKey]; dirty = true; }
    }

    foreach (var i in await db.Items.Where(i => i.ImageUrl != "").ToListAsync())
    {
      var key = LocalKeyForUrl(i.ImageUrl);
      if (key != null && i.ImageUrl != img[key]) { i.ImageUrl = img[key]; dirty = true; }
    }

    if (dirty)
      await db.SaveChangesAsync();
  }
}
