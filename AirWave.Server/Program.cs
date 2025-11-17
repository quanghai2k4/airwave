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
        options.UseSqlite(dbSettings.ConnectionString,
            sqliteOptions => sqliteOptions.MigrationsAssembly("AirWave.Shared")));

    builder.Services.Configure<MqttSettings>(builder.Configuration.GetSection("MqttSettings"));

    builder.Services.AddHostedService<Worker>();

    Log.Information("Building host...");
    var host = builder.Build();

    Log.Information("Running database migrations...");
    using (var scope = host.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AqiDbContext>();
        
        // Check if database was created with EnsureCreated() (no migrations history)
        var canConnect = dbContext.Database.CanConnect();
        var hasMigrationsTable = canConnect && dbContext.Database.GetAppliedMigrations().Any();
        
        if (canConnect && !hasMigrationsTable)
        {
            Log.Warning("Database exists but has no migration history. This database was likely created with EnsureCreated().");
            Log.Warning("Deleting old database to ensure proper migration...");
            
            // Delete the old database file
            dbContext.Database.EnsureDeleted();
            Log.Information("Old database deleted. Creating new database with migrations...");
        }
        
        dbContext.Database.Migrate();
        Log.Information("Database migrations completed successfully");
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
