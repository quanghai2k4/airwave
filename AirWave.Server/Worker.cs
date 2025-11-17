using AirWave.Shared.Configuration;
using AirWave.Shared.Data;
using AirWave.Shared.Models;
using AirWave.Shared.Services;
using Microsoft.Extensions.Options;
using MQTTnet.Client;
using System.Text;

namespace AirWave.Server;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MqttSettings _mqttSettings;
    private MqttClientService? _mqttClient;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, IOptions<MqttSettings> mqttSettings)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _mqttSettings = mqttSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _mqttClient = new MqttClientService(
            _mqttSettings.BrokerHost,
            _mqttSettings.BrokerPort,
            $"AirWave_Server_{Guid.NewGuid()}",
            _logger);

        var connected = await _mqttClient.ConnectAsync(stoppingToken);
        if (!connected)
        {
            _logger.LogError("Failed to connect to MQTT broker. Exiting worker...");
            return;
        }

        _logger.LogInformation("Connected to MQTT broker: {BrokerHost}:{BrokerPort}", _mqttSettings.BrokerHost, _mqttSettings.BrokerPort);

        var subscribed = await _mqttClient.SubscribeAsync(_mqttSettings.Topic, OnMessageReceivedAsync, stoppingToken);
        if (!subscribed)
        {
            _logger.LogError("Failed to subscribe to topic. Exiting worker...");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        await _mqttClient.DisconnectAsync();
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
        _logger.LogInformation("Received message: {Payload}", payload);

        try
        {
            var aqiValue = int.Parse(payload);
            var aqiData = new AqiData
            {
                AqiValue = aqiValue,
                Timestamp = DateTime.UtcNow
            };

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AqiDbContext>();
            dbContext.AqiRecords.Add(aqiData);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Saved AQI: {AqiValue} at {Timestamp} (UTC)", aqiValue, aqiData.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
        }
    }
}
