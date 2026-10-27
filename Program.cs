using System.Text.Json.Serialization;
using MenuOnline.Data;
using MenuOnline.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddControllers(options =>
    {
        // Permite usar nameof(MetodoAsync) no CreatedAtAction
        options.SuppressAsyncSuffixInActionNames = false;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Vitrine web roda em outra origem/porta (ex.: Vite em localhost:5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<MediaService>();

var app = builder.Build();

// Pasta Assets/ versionada no repo, servida em /uploads (ex.: /uploads/seed/xburger.jpg).
app.UseStaticFiles();
var assetsPath = Path.Combine(builder.Environment.ContentRootPath, "Assets");
if (Directory.Exists(assetsPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(assetsPath),
        RequestPath = "/uploads"
    });
}

// Uploads salvos via POST /api/upload vao para wwwroot/uploads (ignorado pelo git).

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Frontend");

    // Loja de exemplo na primeira execucao
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var publicBaseUrl = builder.Configuration["PublicBaseUrl"] ?? "http://localhost:5099";
    await DbSeeder.SeedAsync(db, publicBaseUrl);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
