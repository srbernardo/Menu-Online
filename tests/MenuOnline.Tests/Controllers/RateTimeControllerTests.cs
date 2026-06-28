using MenuOnline.Controllers;
using MenuOnline.Data;
using MenuOnline.Dtos.RateTime;
using MenuOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Tests.Controllers;

public class RateTimeControllerTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static RateTimeController CreateController(AppDbContext context)
    {
        return new RateTimeController(context, null!);
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
    public async Task GetByUser_HasRates_ReturnsOkWithList()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);
        context.RateTimes.Add(new RateTime
        {
            Id = 1,
            Km = 5m,
            TimeInMinutes = "30 min",
            Rate = 10m,
            UserId = 1
        });
        context.RateTimes.Add(new RateTime
        {
            Id = 2,
            Km = 10m,
            TimeInMinutes = "60 min",
            Rate = 20m,
            UserId = 1
        });
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetByUserAsync(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<RateTimeDto>>(okResult.Value);
        Assert.Equal(2, list.Count);
        var dto = list.First(r => r.Id == 1);
        Assert.Equal(5m, dto.Km);
        Assert.Equal("30 min", dto.TimeInMinutes);
        Assert.Equal(10m, dto.Rate);
        Assert.Equal(1, dto.UserId);
    }

    [Fact]
    public async Task GetByUser_NoRates_ReturnsEmptyList()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetByUserAsync(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<RateTimeDto>>(okResult.Value);
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

        var dto = new UpsertRateTimeRequestDto(Km: 3m, TimeInMinutes: "20 min", Rate: 8m, UserId: 1);

        var result = await controller.CreateAsync(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var dtoResult = Assert.IsType<RateTimeDto>(createdResult.Value);
        Assert.Equal(3m, dtoResult.Km);
        Assert.Equal("20 min", dtoResult.TimeInMinutes);
        Assert.Equal(8m, dtoResult.Rate);
        Assert.Equal(1, dtoResult.UserId);
    }

    [Fact]
    public async Task Create_NonExistentUser_ReturnsBadRequest()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var controller = CreateController(context);

        var dto = new UpsertRateTimeRequestDto(Km: 1m, TimeInMinutes: "10 min", Rate: 5m, UserId: 999);

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
        context.RateTimes.Add(new RateTime
        {
            Id = 1, Km = 1m, TimeInMinutes = "10 min", Rate = 5m, UserId = 1
        });
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var dto = new UpsertRateTimeRequestDto(Km: 10m, TimeInMinutes: "60 min", Rate: 25m, UserId: 1);

        var result = await controller.UpdateAsync(1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var dtoResult = Assert.IsType<RateTimeDto>(okResult.Value);
        Assert.Equal(10m, dtoResult.Km);
        Assert.Equal(25m, dtoResult.Rate);
    }

    [Fact]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var controller = CreateController(context);

        var dto = new UpsertRateTimeRequestDto(Km: 1m, TimeInMinutes: "10 min", Rate: 5m, UserId: 1);

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
        context.RateTimes.Add(new RateTime
        {
            Id = 1, Km = 1m, TimeInMinutes = "10 min", Rate = 5m, UserId = 1
        });
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.DeleteAsync(1);

        Assert.IsType<OkObjectResult>(result);
        Assert.Null(await context.RateTimes.FirstOrDefaultAsync(r => r.Id == 1));
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
