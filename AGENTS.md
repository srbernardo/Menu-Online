# MenuOnline - Dev Notes

## Build & Test

```powershell
# Build entire solution
dotnet build

# Run all tests
dotnet test tests\MenuOnline.Tests\MenuOnline.Tests.csproj

# Run single test
dotnet test tests\MenuOnline.Tests\MenuOnline.Tests.csproj --filter "FullyQualifiedName~GetBySlug_ValidSlug"
```

## Test Project

- `tests/MenuOnline.Tests/` — xUnit v2, EF Core InMemory
- `RestaurantControllerTests.cs` — 9 tests covering `GetBySlug`
- No mocking libs (InMemory replaces the real DB)
- Each test uses a unique `Guid` database name for isolation

## Key Fixes

- `User.AddressId` and `User.BusinessHoursId` changed from `int` to `int?` to make 1:1 relationships optional (ENABLE_LOG would have INNER JOINed otherwise)
- `MenuOnline.csproj` excludes `tests/**` from compilation via `<Compile Remove="tests\**\*" />`
