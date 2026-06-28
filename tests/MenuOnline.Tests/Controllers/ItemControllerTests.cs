using MenuOnline.Controllers;
using MenuOnline.Data;
using MenuOnline.Dtos.Item;
using MenuOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;

namespace MenuOnline.Tests.Controllers;

public class ItemControllerTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    private static ItemController CreateController(AppDbContext context)
    {
        return new ItemController(context, NullLogger<ItemController>.Instance);
    }

    private static User CreateUser(int id = 1)
    {
        return new User
        {
            Id = id,
            Slug = $"user-{id}",
            Name = "Test User",
            Email = "test@user.com",
            Status = User.StatusUser.Active,
            Role = User.RoleUser.Loja
        };
    }

    [Fact]
    public async Task GetAll_UserHasItems_ReturnsOkWithList()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);

        var category = new Category { Id = 1, Title = "Bebidas", UserId = 1 };
        context.Categories.Add(category);

        context.Items.AddRange(
            new Item { Id = 1, Title = "Coca", Price = 5m, CategoryId = 1, UserId = 1 },
            new Item { Id = 2, Title = "Pizza", Price = 20m, CategoryId = 1, UserId = 1 }
        );
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetAllAsync(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<ItemDto>>(okResult.Value);
        Assert.Equal(2, list.Count);
        Assert.Contains(list, i => i.Title == "Coca");
        Assert.Contains(list, i => i.Title == "Pizza");
    }

    [Fact]
    public async Task GetAll_UserHasNoItems_ReturnsEmptyList()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetAllAsync(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<ItemDto>>(okResult.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task Create_Valid_ReturnsCreated()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);

        var category = new Category { Id = 1, Title = "Bebidas", UserId = 1 };
        context.Categories.Add(category);

        var extra = new Extra { Id = 1, Title = "Gelo", Description = "Gelo extra", Value = 1m, UserId = 1 };
        context.Extras.Add(extra);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var dto = new UpsetItemRequestDto(
            Description: "Refrigerante de cola",
            Title: "Coca-Cola",
            Price: 5m,
            CategoryId: 1,
            ExtraIds: new List<int> { 1 },
            ImageUrl: "",
            UserId: 1
        );

        var result = await controller.CreateAsync(dto);

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal("Item criado com sucesso!", createdResult.Location);

        var itemExtra = await context.ItemExtras.FirstOrDefaultAsync(ie => ie.ExtraId == 1);
        Assert.NotNull(itemExtra);
    }

    [Fact]
    public async Task Create_NonExistentUser_ReturnsBadRequest()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var controller = CreateController(context);

        var dto = new UpsetItemRequestDto(
            Description: "Test",
            Title: "Test",
            Price: 5m,
            CategoryId: 1,
            ExtraIds: new List<int>(),
            ImageUrl: "",
            UserId: 999
        );

        var result = await controller.CreateAsync(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_NonExistentCategory_ReturnsBadRequest()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var dto = new UpsetItemRequestDto(
            Description: "Test",
            Title: "Test",
            Price: 5m,
            CategoryId: 999,
            ExtraIds: new List<int>(),
            ImageUrl: "",
            UserId: 1
        );

        var result = await controller.CreateAsync(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_NonExistentExtra_ReturnsBadRequest()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);

        var category = new Category { Id = 1, Title = "Cat", UserId = 1 };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var dto = new UpsetItemRequestDto(
            Description: "Test",
            Title: "Test",
            Price: 5m,
            CategoryId: 1,
            ExtraIds: new List<int> { 999 },
            ImageUrl: "",
            UserId: 1
        );

        var result = await controller.CreateAsync(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_Existing_ReturnsOk()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);

        var category = new Category { Id = 1, Title = "Cat", UserId = 1 };
        context.Categories.Add(category);

        context.Items.Add(new Item { Id = 1, Title = "Old", Price = 5m, CategoryId = 1, UserId = 1 });
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var dto = new UpsetItemRequestDto(
            Description: "New desc",
            Title: "New Title",
            Price: 10m,
            CategoryId: 1,
            ExtraIds: new List<int>(),
            ImageUrl: "",
            UserId: 1
        );

        var result = await controller.UpdateAsync(1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var dtoResult = Assert.IsType<UpsetItemRequestDto>(okResult.Value);
        Assert.Equal("New Title", dtoResult.Title);
    }

    [Fact]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var controller = CreateController(context);

        var dto = new UpsetItemRequestDto(
            Description: "Test",
            Title: "Test",
            Price: 5m,
            CategoryId: 1,
            ExtraIds: new List<int>(),
            ImageUrl: "",
            UserId: 1
        );

        var result = await controller.UpdateAsync(999, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Existing_ReturnsOk()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);

        var category = new Category { Id = 1, Title = "Cat", UserId = 1 };
        context.Categories.Add(category);

        context.Items.Add(new Item { Id = 1, Title = "Test", Price = 5m, CategoryId = 1, UserId = 1 });
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.DeleteAsync(1);

        Assert.IsType<OkObjectResult>(result);
        Assert.Null(await context.Items.FirstOrDefaultAsync(i => i.Id == 1));
    }

    [Fact]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var controller = CreateController(context);

        var result = await controller.DeleteAsync(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
