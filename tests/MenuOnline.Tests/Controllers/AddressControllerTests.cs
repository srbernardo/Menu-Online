using MenuOnline.Controllers;
using MenuOnline.Data;
using MenuOnline.Dtos.Address;
using MenuOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace MenuOnline.Tests.Controllers;

public class AddressControllerTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static AddressController CreateController(AppDbContext context)
    {
        return new AddressController(context, null!);
    }

    private static User CreateUser(int id = 1, string slug = "test-user")
    {
        return new User
        {
            Id = id,
            Slug = slug,
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
        var address = new Address
        {
            Id = 1,
            cep = "12345-678",
            street = "Rua A",
            number = "100",
            complement = "Apto 1",
            neighborhood = "Centro",
            city = "Sao Paulo",
            state = Enums.States.SP,
            referencePoint = "Praca",
            UserId = 1
        };
        context.Addresses.Add(address);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.GetByUserAsync(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<AddressDto>(okResult.Value);
        Assert.Equal(1, dto.Id);
        Assert.Equal("12345-678", dto.Cep);
        Assert.Equal("Rua A", dto.Street);
        Assert.Equal("100", dto.Number);
        Assert.Equal("Apto 1", dto.Complement);
        Assert.Equal("Centro", dto.Neighborhood);
        Assert.Equal("Sao Paulo", dto.City);
        Assert.Equal("SP", dto.State);
        Assert.Equal("Praca", dto.ReferencePoint);
        Assert.Equal(1, dto.UserId);
    }

    [Fact]
    public async Task GetByUser_NonExistent_ReturnsNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var controller = CreateController(context);

        var result = await controller.GetByUserAsync(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ValidAddress_ReturnsCreated()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var dto = new UpsertAddressRequestDto(
            Cep: "12345-678",
            Street: "Rua A",
            Number: "100",
            Complement: "Apto 1",
            Neighborhood: "Centro",
            City: "Sao Paulo",
            State: Enums.States.SP,
            ReferencePoint: "Praca",
            UserId: 1
        );

        var result = await controller.CreateAsync(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var addressDto = Assert.IsType<AddressDto>(createdResult.Value);
        Assert.Equal("12345-678", addressDto.Cep);
        Assert.Equal("Rua A", addressDto.Street);
        Assert.Equal("Sao Paulo", addressDto.City);
        Assert.Equal("SP", addressDto.State);
        Assert.Equal(1, addressDto.UserId);
        Assert.True(addressDto.Id > 0);

        var saved = await context.Addresses.FirstOrDefaultAsync(a => a.Id == addressDto.Id);
        Assert.NotNull(saved);
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

        var dto = new UpsertAddressRequestDto(
            Cep: "",
            Street: "",
            Number: "",
            Complement: "",
            Neighborhood: "",
            City: "",
            State: Enums.States.SP,
            ReferencePoint: "",
            UserId: 1
        );

        var result = await controller.CreateAsync(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_NonExistentUser_ReturnsBadRequest()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var controller = CreateController(context);

        var dto = new UpsertAddressRequestDto(
            Cep: "12345-678",
            Street: "Rua A",
            Number: "100",
            Complement: "",
            Neighborhood: "Centro",
            City: "Sao Paulo",
            State: Enums.States.SP,
            ReferencePoint: "",
            UserId: 999
        );

        var result = await controller.CreateAsync(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_ExistingAddress_ReturnsOk()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);
        var address = new Address
        {
            Id = 1,
            cep = "12345-678",
            street = "Rua A",
            number = "100",
            neighborhood = "Centro",
            city = "Sao Paulo",
            state = Enums.States.SP,
            UserId = 1
        };
        context.Addresses.Add(address);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var dto = new UpsertAddressRequestDto(
            Cep: "11111-111",
            Street: "Rua Nova",
            Number: "999",
            Complement: "Casa",
            Neighborhood: "Novo Bairro",
            City: "Nova Cidade",
            State: Enums.States.RJ,
            ReferencePoint: "Esquina",
            UserId: 1
        );

        var result = await controller.UpdateAsync(1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var addressDto = Assert.IsType<AddressDto>(okResult.Value);
        Assert.Equal("11111-111", addressDto.Cep);
        Assert.Equal("Rua Nova", addressDto.Street);
        Assert.Equal("999", addressDto.Number);
        Assert.Equal("Nova Cidade", addressDto.City);
        Assert.Equal("RJ", addressDto.State);

        var saved = await context.Addresses.AsNoTracking().FirstAsync(a => a.Id == 1);
        Assert.Equal("11111-111", saved.cep);
    }

    [Fact]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var controller = CreateController(context);

        var dto = new UpsertAddressRequestDto(
            Cep: "12345-678",
            Street: "Rua",
            Number: "1",
            Complement: "",
            Neighborhood: "Centro",
            City: "Cidade",
            State: Enums.States.SP,
            ReferencePoint: "",
            UserId: 1
        );

        var result = await controller.UpdateAsync(999, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ExistingAddress_ReturnsOk()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = CreateUser();
        context.Users.Add(user);
        var address = new Address
        {
            Id = 1,
            cep = "12345-678",
            street = "Rua A",
            number = "100",
            neighborhood = "Centro",
            city = "Sao Paulo",
            state = Enums.States.SP,
            UserId = 1
        };
        context.Addresses.Add(address);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result = await controller.DeleteAsync(1);

        Assert.IsType<OkObjectResult>(result);

        var deleted = await context.Addresses.FirstOrDefaultAsync(a => a.Id == 1);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var controller = CreateController(context);

        var result = await controller.DeleteAsync(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
