using Microsoft.EntityFrameworkCore;
using QuickBin.Domain.Configurations;
using QuickBin.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<QuickBinDbContext>(context =>
{
    var dbConfig = builder.Configuration.GetSection("QuickBin:DB").Get<DatabaseConfiguration>();
    if (dbConfig == null)
        throw new ArgumentException("Database configuration not found");

    var connectionString = $"Host={dbConfig.Host}:{dbConfig.Port}; Database={dbConfig.Database}; Username={dbConfig.User}; Password={dbConfig.Password}";
    context.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    context.UseNpgsql(connectionString);
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<QuickBinDbContext>();
context.Database.Migrate();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();