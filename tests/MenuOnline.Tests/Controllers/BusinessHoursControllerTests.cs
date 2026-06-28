using MenuOnline.Controllers;
using MenuOnline.Data;
using MenuOnline.Dtos.BusinessHours;
using MenuOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Tests.Controllers;

public class BusinessHoursControllerTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static BusinessHoursController CreateController(AppDbContext context)
    {
        return new BusinessHoursController(context, null!);
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
    public async Task GetByUser_Existing_ReturnsOk()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);
        var bh = new BusinessHours
        {
            Id = 1,
            UserId = 1,
            Monday = "08:00-18:00",
            Tuesday = "08:00-18:00",
            Wednesday = "08:00-18:00",
            Thursday = "08:00-18:00",
            Friday = "08:00-18:00",
            Saturday = "09:00-13:00",
            Sunday = ""
        };
        context.BusinessHours.Add(bh);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetByUserAsync(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<BusinessHoursDto>(okResult.Value);
        Assert.Equal(1, dto.Id);
        Assert.Equal(1, dto.UserId);
        Assert.Equal("08:00-18:00", dto.Monday);
        Assert.Equal("", dto.Sunday);
    }

    [Fact]
    public async Task GetByUser_NonExistent_ReturnsNotFound()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var controller = CreateController(context);

        var result = await controller.GetByUserAsync(999);

        Assert.IsType<NotFoundResult>(result);
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

        var dto = new UpsertBusinessHoursRequestDto(
            UserId: 1,
            Monday: "08:00-18:00",
            Tuesday: "08:00-18:00",
            Wednesday: "08:00-18:00",
            Thursday: "08:00-18:00",
            Friday: "08:00-18:00",
            Saturday: "09:00-13:00",
            Sunday: ""
        );

        var result = await controller.CreateAsync(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var dtoResult = Assert.IsType<BusinessHoursDto>(createdResult.Value);
        Assert.Equal(1, dtoResult.UserId);
        Assert.Equal("08:00-18:00", dtoResult.Monday);
    }

    [Fact]
    public async Task Create_NonExistentUser_ReturnsBadRequest()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var controller = CreateController(context);

        var dto = new UpsertBusinessHoursRequestDto(999, "", "", "", "", "", "", "");

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
        var bh = new BusinessHours
        {
            Id = 1,
            UserId = 1,
            Monday = "old",
            Tuesday = "", Wednesday = "", Thursday = "", Friday = "", Saturday = "", Sunday = ""
        };
        context.BusinessHours.Add(bh);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var dto = new UpsertBusinessHoursRequestDto(1, "new", "", "", "", "", "", "");

        var result = await controller.UpdateAsync(1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var dtoResult = Assert.IsType<BusinessHoursDto>(okResult.Value);
        Assert.Equal("new", dtoResult.Monday);
    }

    [Fact]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var controller = CreateController(context);

        var dto = new UpsertBusinessHoursRequestDto(1, "", "", "", "", "", "", "");

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
        var bh = new BusinessHours
        {
            Id = 1, UserId = 1,
            Monday = "", Tuesday = "", Wednesday = "", Thursday = "",
            Friday = "", Saturday = "", Sunday = ""
        };
        context.BusinessHours.Add(bh);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.DeleteAsync(1);

        Assert.IsType<OkObjectResult>(result);
        Assert.Null(await context.BusinessHours.FirstOrDefaultAsync(x => x.Id == 1));
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
