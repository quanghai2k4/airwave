using AirWave.Shared.Configuration;
using AirWave.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

Console.WriteLine("AirWave Sensor - Starting...");

// Setup logging
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<Program>();

// Load configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var mqttSettings = configuration.GetSection("MqttSettings").Get<MqttSettings>() ?? new MqttSettings();

// Create MQTT client with reconnection support
var mqttClient = new MqttClientService(
    mqttSettings.BrokerHost,
    mqttSettings.BrokerPort,
    $"AirWave_Sensor_{Guid.NewGuid()}",
    logger);

// Connect to MQTT broker
var connected = await mqttClient.ConnectAsync();
if (!connected)
{
    logger.LogError("Failed to connect to MQTT broker. Exiting...");
    return;
}

logger.LogInformation("Connected to MQTT broker: {BrokerHost}:{BrokerPort}", mqttSettings.BrokerHost, mqttSettings.BrokerPort);
logger.LogInformation("Publishing AQI data every {Interval} seconds...", mqttSettings.PublishIntervalSeconds);

var random = new Random();

while (true)
{
    try
    {
        var aqiValue = random.Next(0, 200);

        var published = await mqttClient.PublishAsync(mqttSettings.Topic, aqiValue.ToString());
        
        if (published)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            logger.LogInformation("[{Timestamp}] Published AQI: {AqiValue}", timestamp, aqiValue);
        }
        else
        {
            logger.LogWarning("Failed to publish AQI value. Will retry in next interval.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error in publishing loop");
    }

    await Task.Delay(mqttSettings.PublishIntervalSeconds * 1000);
}

