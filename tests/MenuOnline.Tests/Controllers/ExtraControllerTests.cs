using MenuOnline.Controllers;
using MenuOnline.Data;
using MenuOnline.Dtos.Extra;
using MenuOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Tests.Controllers;

public class ExtraControllerTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static ExtraController CreateController(AppDbContext context)
    {
        return new ExtraController(context, null!);
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
    public async Task GetAll_ItemHasExtras_ReturnsOkWithList()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);

        var category = new Category { Id = 1, Title = "Cat", UserId = 1 };
        context.Categories.Add(category);

        var item = new Item
        {
            Id = 1,
            Title = "Item",
            Price = 10m,
            CategoryId = 1,
            UserId = 1
        };
        context.Items.Add(item);

        var extra1 = new Extra { Id = 1, Title = "Extra 1", Description = "Desc 1", Value = 1m, UserId = 1 };
        var extra2 = new Extra { Id = 2, Title = "Extra 2", Description = "Desc 2", Value = 2m, UserId = 1 };
        context.Extras.AddRange(extra1, extra2);

        context.ItemExtras.AddRange(
            new ItemExtra { ItemId = 1, ExtraId = 1 },
            new ItemExtra { ItemId = 1, ExtraId = 2 }
        );
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetAllAsync(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<ExtraDto>>(okResult.Value);
        Assert.Equal(2, list.Count);
        Assert.Contains(list, e => e.Title == "Extra 1");
        Assert.Contains(list, e => e.Title == "Extra 2");
    }

    [Fact]
    public async Task GetAll_ItemHasNoExtras_ReturnsEmptyList()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);
        var item = new Item { Id = 1, Title = "Item", Price = 10m, CategoryId = 1, UserId = 1 };
        context.Items.Add(item);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetAllAsync(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<ExtraDto>>(okResult.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task Create_Valid_ReturnsCreated()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var item = new Item { Id = 1, Title = "Pizza", Price = 20m, CategoryId = 1, UserId = 1 };
        context.Items.Add(item);
        await context.SaveChangesAsync();

        var dto = new UpsertExtraRequestDto(Description: "Queijo extra", Title: "Extra Cheese", Value: 3m, UserId: 1, ItemId: 1);

        var result = await controller.CreateAsync(dto);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var dtoResult = Assert.IsType<ExtraDto>(createdResult.Value);
        Assert.Equal("Extra Cheese", dtoResult.Title);
        Assert.Equal(3m, dtoResult.Value);

        var itemExtra = await context.ItemExtras.FirstOrDefaultAsync(ie => ie.ExtraId == dtoResult.Id && ie.ItemId == 1);
        Assert.NotNull(itemExtra);
    }

    [Fact]
    public async Task Create_NonExistentUser_ReturnsBadRequest()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var controller = CreateController(context);

        var dto = new UpsertExtraRequestDto(Description: "Test", Title: "Test", Value: 1m, UserId: 999, ItemId: 1);

        var result = await controller.CreateAsync(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_NonExistentItem_ReturnsBadRequest()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var controller = CreateController(context);

        var dto = new UpsertExtraRequestDto(Description: "Test", Title: "Test", Value: 1m, UserId: 1, ItemId: 999);

        var result = await controller.CreateAsync(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_MissingRequiredFields_ReturnsBadRequest()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var dto = new UpsertExtraRequestDto(Description: "", Title: "", Value: 0, UserId: 1, ItemId: 0);

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
        context.Extras.Add(new Extra { Id = 1, Title = "Old", Description = "Old", Value = 1m, UserId = 1 });
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var dto = new UpsertExtraRequestDto(Description: "New desc", Title: "New Title", Value: 9.99m, UserId: 1, ItemId: 1);

        var result = await controller.UpdateAsync(1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var dtoResult = Assert.IsType<ExtraDto>(okResult.Value);
        Assert.Equal("New Title", dtoResult.Title);
        Assert.Equal(9.99m, dtoResult.Value);
    }

    [Fact]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var controller = CreateController(context);

        var dto = new UpsertExtraRequestDto(Description: "Test", Title: "Test", Value: 1m, UserId: 1, ItemId: 1);

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
        context.Extras.Add(new Extra { Id = 1, Title = "Test", Description = "Desc", Value = 1m, UserId = 1 });
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.DeleteAsync(1);

        Assert.IsType<OkObjectResult>(result);
        Assert.Null(await context.Extras.FirstOrDefaultAsync(e => e.Id == 1));
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
