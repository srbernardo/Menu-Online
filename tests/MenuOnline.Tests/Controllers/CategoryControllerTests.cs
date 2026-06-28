using MenuOnline.Controllers;
using MenuOnline.Data;
using MenuOnline.Dtos.Category;
using MenuOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Tests.Controllers;

public class CategoryControllerTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static CategoryController CreateController(AppDbContext context)
    {
        return new CategoryController(context, null!);
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
    public async Task GetAll_UserHasCategories_ReturnsOkWithList()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);
        context.Categories.AddRange(
            new Category { Id = 1, Title = "Bebidas", UserId = 1 },
            new Category { Id = 2, Title = "Pizzas", UserId = 1 }
        );
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetAllAsync(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<CategoryDto>>(okResult.Value);
        Assert.Equal(2, list.Count);
        Assert.Contains(list, c => c.Title == "Bebidas");
        Assert.Contains(list, c => c.Title == "Pizzas");
    }

    [Fact]
    public async Task GetAll_UserHasNoCategories_ReturnsEmptyList()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetAllAsync(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<CategoryDto>>(okResult.Value);
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

        var dto = new UpsertCategoryRequestDto(Title: "Sobremesas", UserId: 1);

        var result = await controller.CreateAsync(dto);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var dtoResult = Assert.IsType<CategoryDto>(createdResult.Value);
        Assert.Equal("Sobremesas", dtoResult.Title);
        Assert.True(dtoResult.Id > 0);
    }

    [Fact]
    public async Task Create_NonExistentUser_ReturnsBadRequest()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var controller = CreateController(context);

        var dto = new UpsertCategoryRequestDto(Title: "Test", UserId: 999);

        var result = await controller.CreateAsync(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_MissingTitle_ReturnsBadRequest()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var controller = CreateController(context);

        var dto = new UpsertCategoryRequestDto(Title: "", UserId: 1);

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
        context.Categories.Add(new Category { Id = 1, Title = "Old", UserId = 1 });
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var dto = new UpsertCategoryRequestDto(Title: "Updated", UserId: 1);

        var result = await controller.UpdateAsync(1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var dtoResult = Assert.IsType<CategoryDto>(okResult.Value);
        Assert.Equal("Updated", dtoResult.Title);
    }

    [Fact]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var controller = CreateController(context);

        var dto = new UpsertCategoryRequestDto(Title: "Test", UserId: 1);

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
        context.Categories.Add(new Category { Id = 1, Title = "Test", UserId = 1 });
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.DeleteAsync(1);

        Assert.IsType<OkObjectResult>(result);
        Assert.Null(await context.Categories.FirstOrDefaultAsync(c => c.Id == 1));
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
