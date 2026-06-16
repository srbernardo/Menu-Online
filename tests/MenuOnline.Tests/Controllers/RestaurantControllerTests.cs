using MenuOnline.Controllers;
using MenuOnline.Data;
using MenuOnline.Dtos.Restaurant;
using MenuOnline.Enums;
using MenuOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Tests.Controllers;

public class RestaurantControllerTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static RestaurantController CreateController(AppDbContext context)
    {
        return new RestaurantController(context);
    }

    private static Address CreateAddress(int id = 1, int userId = 1) => new()
    {
        Id = id,
        cep = "12345-678",
        street = "Rua A",
        number = "100",
        complement = "Apto 1",
        neighborhood = "Centro",
        city = "Sao Paulo",
        state = States.SP,
        referencePoint = "Praca",
        UserId = userId
    };

    private static BusinessHours CreateBusinessHours(int id = 1, int userId = 1) => new()
    {
        Id = id,
        UserId = userId,
        Monday = "08:00-18:00",
        Tuesday = "08:00-18:00",
        Wednesday = "08:00-18:00",
        Thursday = "08:00-18:00",
        Friday = "08:00-18:00",
        Saturday = "09:00-13:00",
        Sunday = ""
    };

    [Fact]
    public async Task GetBySlug_ValidSlug_ReturnsOkWithProfile()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var category = new Category { Id = 1, Title = "Bebidas" };
        var extra = new Extra { Id = 1, Description = "Lata 350ml", Title = "Coca-Cola Extra", Value = 2.5m };
        var itemExtra = new ItemExtra { ItemId = 1, ExtraId = 1 };
        var item = new Item
        {
            Id = 1,
            Title = "Coca-Cola",
            Description = "Refrigerante de cola",
            ImageUrl = "coca.jpg",
            Price = 5m,
            CategoryId = 1,
            Category = category,
            UserId = 1,
            ItemExtras = [itemExtra]
        };
        itemExtra.Item = item;
        itemExtra.Extra = extra;

        var address = CreateAddress();
        var businessHours = CreateBusinessHours();
        var rateTime = new RateTime
        {
            Id = 1,
            Km = 5m,
            TimeInMinutes = "30 min",
            Rate = 10m,
            UserId = 1
        };

        var user = new User
        {
            Id = 1,
            Slug = "my-restaurant",
            Name = "My Restaurant",
            Description = "Best food ever",
            WhatsappNumber = "11999999999",
            Status = User.StatusUser.Active,
            Role = User.RoleUser.Loja,
            Email = "restaurant@test.com",
            Address = address,
            BusinessHours = businessHours,
            Items = [item],
            RateTimes = [rateTime]
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetBySlug("my-restaurant");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<RestaurantProfileDto>(okResult.Value);
        Assert.Equal(1, dto.Id);
        Assert.Equal("my-restaurant", dto.Slug);
        Assert.Equal("My Restaurant", dto.Name);
        Assert.Equal("Best food ever", dto.Description);
        Assert.Equal("11999999999", dto.WhatsappNumber);

        Assert.NotNull(dto.Address);
        Assert.Equal("12345-678", dto.Address.Cep);
        Assert.Equal("Sao Paulo", dto.Address.City);
        Assert.Equal("SP", dto.Address.State);

        Assert.NotNull(dto.BusinessHours);
        Assert.Equal("08:00-18:00", dto.BusinessHours.Monday);
        Assert.Equal("", dto.BusinessHours.Sunday);

        var rateTimeDto = Assert.Single(dto.RateTimes);
        Assert.Equal(5m, rateTimeDto.Km);
        Assert.Equal("30 min", rateTimeDto.TimeInMinutes);
        Assert.Equal(10m, rateTimeDto.Rate);

        var itemDto = Assert.Single(dto.Items);
        Assert.Equal("Coca-Cola", itemDto.Title);
        Assert.Equal("Bebidas", itemDto.CategoryName);

        var extraDto = Assert.Single(itemDto.Extras);
        Assert.Equal("Coca-Cola Extra", extraDto.Title);
        Assert.Equal(2.5m, extraDto.Value);
    }

    [Fact]
    public async Task GetBySlug_ValidSlug_NoItemsNoRateTimes_ReturnsOk()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = new User
        {
            Id = 1,
            Slug = "minimal",
            Name = "Minimal",
            Description = "",
            WhatsappNumber = "",
            Status = User.StatusUser.Active,
            Role = User.RoleUser.Loja,
            Email = "minimal@test.com",
            Address = CreateAddress(),
            BusinessHours = CreateBusinessHours()
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetBySlug("minimal");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<RestaurantProfileDto>(okResult.Value);
        Assert.Equal("minimal", dto.Slug);
        Assert.NotNull(dto.Address);
        Assert.NotNull(dto.BusinessHours);
        Assert.Empty(dto.RateTimes);
        Assert.Empty(dto.Items);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetBySlug_InvalidSlug_ReturnsBadRequest(string? slug)
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var controller = CreateController(context);

        var result = await controller.GetBySlug(slug!);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetBySlug_NonExistentSlug_ReturnsNotFound()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());

        var user = new User
        {
            Id = 1,
            Slug = "existing",
            Name = "Existing",
            Status = User.StatusUser.Active,
            Role = User.RoleUser.Loja,
            Email = "existing@test.com",
            Address = CreateAddress(),
            BusinessHours = CreateBusinessHours()
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetBySlug("no-such");

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Theory]
    [InlineData(User.StatusUser.Inactive)]
    [InlineData(User.StatusUser.Suspended)]
    public async Task GetBySlug_NonActiveUser_ReturnsNotFound(User.StatusUser status)
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = new User
        {
            Id = 1,
            Slug = "non-active",
            Name = "Non Active",
            Status = status,
            Role = User.RoleUser.Loja,
            Email = "nonactive@test.com",
            Address = CreateAddress(),
            BusinessHours = CreateBusinessHours()
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetBySlug("non-active");

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetBySlug_ItemsWithExtras_ReturnsOkWithExtras()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var category = new Category { Id = 1, Title = "Pizzas" };
        var extra1 = new Extra { Id = 1, Description = "Queijo extra", Title = "Extra Cheese", Value = 3m };
        var extra2 = new Extra { Id = 2, Description = "Borda recheada", Title = "Stuffed Crust", Value = 5m };
        var itemExtra1 = new ItemExtra { ItemId = 1, ExtraId = 1 };
        var itemExtra2 = new ItemExtra { ItemId = 1, ExtraId = 2 };
        var item = new Item
        {
            Id = 1,
            Title = "Calabresa",
            Description = "Pizza de calabresa com cebola",
            ImageUrl = "calabresa.jpg",
            Price = 35m,
            CategoryId = 1,
            Category = category,
            UserId = 1,
            ItemExtras = [itemExtra1, itemExtra2]
        };
        itemExtra1.Item = item;
        itemExtra1.Extra = extra1;
        itemExtra2.Item = item;
        itemExtra2.Extra = extra2;

        var user = new User
        {
            Id = 1,
            Slug = "pizza-place",
            Name = "Pizza Place",
            Status = User.StatusUser.Active,
            Role = User.RoleUser.Loja,
            Email = "pizza@test.com",
            Address = CreateAddress(),
            BusinessHours = CreateBusinessHours(),
            Items = [item]
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetBySlug("pizza-place");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<RestaurantProfileDto>(okResult.Value);
        var itemDto = Assert.Single(dto.Items);
        Assert.Equal("Calabresa", itemDto.Title);
        Assert.Equal(2, itemDto.Extras.Count);
        Assert.Single(itemDto.Extras, e => e.Title == "Extra Cheese" && e.Value == 3m);
        Assert.Single(itemDto.Extras, e => e.Title == "Stuffed Crust" && e.Value == 5m);
    }
}
