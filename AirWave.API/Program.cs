using AirWave.Shared.Configuration;
using AirWave.Shared.Data;
using AirWave.Shared.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build())
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/airwave-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting AirWave API...");

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

var dbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>() ?? new DatabaseSettings();

builder.Services.AddDbContext<AqiDbContext>(options =>
    options.UseSqlite(dbSettings.ConnectionString, 
        sqliteOptions => sqliteOptions.MigrationsAssembly("AirWave.API")));

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AqiDbContext>("database");

builder.Services.AddControllers();

// Add Response Caching
builder.Services.AddResponseCaching();

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", rateLimiterOptions =>
    {
        rateLimiterOptions.Window = TimeSpan.FromMinutes(1);
        rateLimiterOptions.PermitLimit = 100;
        rateLimiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        rateLimiterOptions.QueueLimit = 10;
    });
});

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<AqiFilterRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5281", "https://localhost:7214")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AqiDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowBlazorClient");
app.UseResponseCaching();
app.UseRateLimiter();
app.UseAuthorization();

// Map Health Check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.MapControllers();

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AirWave API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
