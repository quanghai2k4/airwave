using AirWave.Server;
using AirWave.Shared.Configuration;
using AirWave.Shared.Data;
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
    .WriteTo.File("logs/airwave-server-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting AirWave Server...");

var builder = Host.CreateApplicationBuilder(args);

// Use Serilog
builder.Services.AddSerilog();

var dbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>() ?? new DatabaseSettings();

builder.Services.AddDbContext<AqiDbContext>(options =>
    options.UseSqlite(dbSettings.ConnectionString));

builder.Services.Configure<MqttSettings>(builder.Configuration.GetSection("MqttSettings"));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AqiDbContext>();
    dbContext.Database.EnsureCreated();
}

host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AirWave Server terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
