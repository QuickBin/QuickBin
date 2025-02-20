using Microsoft.EntityFrameworkCore;
using QuickBin.Domain.Configurations;
using QuickBin.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSpaStaticFiles(spaStaticFilesOptions => { spaStaticFilesOptions.RootPath = "wwwroot/browser"; });

builder.Services.AddDbContext<QuickBinDbContext>(context =>
{
    var dbConfig = builder.Configuration.GetSection("QuickBin:DB").Get<DatabaseConfiguration>();
    if (dbConfig == null)
        throw new ArgumentException("Database configuration not found");

    var connectionString = $"Host={dbConfig.Host}:{dbConfig.Port}; Database={dbConfig.Database}; Username={dbConfig.User}; Password={dbConfig.Password}";
    context.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    context.UseNpgsql(connectionString);
});


if (builder.Environment.IsDevelopment())
{
    // cors
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(corsBuilder =>
        {
            corsBuilder.WithOrigins("http://localhost:4200");
            corsBuilder.WithExposedHeaders("Content-Disposition");
            corsBuilder.AllowAnyHeader();
            corsBuilder.AllowAnyMethod();
            corsBuilder.AllowCredentials();
            if (!builder.Environment.IsProduction())
            {
                corsBuilder.WithExposedHeaders("X-Impersonate");
            }
        });
    });
}

var app = builder.Build();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<QuickBinDbContext>();
context.Database.Migrate();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.MapOpenApi();
app.MapScalarApiReference();
// }

app.UseHttpsRedirection();
app.UseStaticFiles();
if (!app.Environment.IsDevelopment())
{
    app.UseSpaStaticFiles();
}
app.UseRouting();
app.UseCors();

app.UseAuthorization();

app.MapControllers();

if (!app.Environment.IsDevelopment())
{
    app.UseSpa(spa =>
    {
        // To learn more about options for serving an Angular SPA from ASP.NET Core,
        // see https://go.microsoft.com/fwlink/?linkid=864501
        spa.Options.SourcePath = "wwwroot";
    });
}

app.Run();