using AirWave.Server;
using AirWave.Shared.Configuration;
using AirWave.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Ensure logs and data directories exist
var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
if (!Directory.Exists(logsPath))
{
    Directory.CreateDirectory(logsPath);
}

var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
if (!Directory.Exists(dataPath))
{
    Directory.CreateDirectory(dataPath);
}

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
    Log.Information("Database connection string: {ConnectionString}", dbSettings.ConnectionString);

    builder.Services.AddDbContext<AqiDbContext>(options =>
        options.UseSqlite(dbSettings.ConnectionString));

    builder.Services.Configure<MqttSettings>(builder.Configuration.GetSection("MqttSettings"));

    builder.Services.AddHostedService<Worker>();

    Log.Information("Building host...");
    var host = builder.Build();

    Log.Information("Ensuring database is created...");
    using (var scope = host.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AqiDbContext>();
        dbContext.Database.EnsureCreated();
        Log.Information("Database created successfully");
    }

    Log.Information("Starting host...");
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
