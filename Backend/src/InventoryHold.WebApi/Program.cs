using InventoryHold.Domain.Repositories;
using InventoryHold.Domain.Services;
using InventoryHold.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhostFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddInventoryHoldInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Inventory Hold API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowLocalhostFrontend");

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var inventoryRepository = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();
    await inventoryRepository.SeedProductsAsync();
}

app.Run();
