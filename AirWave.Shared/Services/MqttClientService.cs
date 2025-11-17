using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;

namespace AirWave.Shared.Services;

public class MqttClientService
{
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _options;
    private readonly ILogger? _logger;
    private bool _isReconnecting = false;

    public bool IsConnected => _mqttClient.IsConnected;

    public MqttClientService(string brokerHost, int brokerPort, string clientId, ILogger? logger = null)
    {
        _logger = logger;
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        _options = new MqttClientOptionsBuilder()
            .WithTcpServer(brokerHost, brokerPort)
            .WithClientId(clientId)
            .WithCleanSession()
            .Build();

        _mqttClient.DisconnectedAsync += OnDisconnectedAsync;
    }

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _mqttClient.ConnectAsync(_options, cancellationToken);
            _logger?.LogInformation("Connected to MQTT broker successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to connect to MQTT broker");
            return false;
        }
    }

    public async Task<bool> PublishAsync(string topic, string payload, CancellationToken cancellationToken = default)
    {
        if (!_mqttClient.IsConnected)
        {
            _logger?.LogWarning("Cannot publish message - not connected to MQTT broker");
            return false;
        }

        try
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithRetainFlag(false)
                .Build();

            await _mqttClient.PublishAsync(message, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error publishing MQTT message");
            return false;
        }
    }

    public async Task<bool> SubscribeAsync(string topic, Func<MqttApplicationMessageReceivedEventArgs, Task> messageHandler, CancellationToken cancellationToken = default)
    {
        if (!_mqttClient.IsConnected)
        {
            _logger?.LogWarning("Cannot subscribe - not connected to MQTT broker");
            return false;
        }

        try
        {
            _mqttClient.ApplicationMessageReceivedAsync += messageHandler;

            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(topic)
                .Build();

            await _mqttClient.SubscribeAsync(subscribeOptions, cancellationToken);
            _logger?.LogInformation("Subscribed to topic: {Topic}", topic);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error subscribing to MQTT topic");
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
                _logger?.LogInformation("Disconnected from MQTT broker");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error disconnecting from MQTT broker");
        }
    }

    private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        if (_isReconnecting)
            return;

        _isReconnecting = true;
        _logger?.LogWarning("Disconnected from MQTT broker. Attempting to reconnect...");

        var delay = TimeSpan.FromSeconds(5);
        var maxDelay = TimeSpan.FromMinutes(5);
        var attempt = 0;

        while (!_mqttClient.IsConnected)
        {
            attempt++;
            try
            {
                await Task.Delay(delay);
                _logger?.LogInformation("Reconnection attempt {Attempt}...", attempt);
                
                await _mqttClient.ConnectAsync(_options);
                _logger?.LogInformation("Reconnected to MQTT broker successfully");
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Reconnection attempt {Attempt} failed", attempt);
                
                // Exponential backoff
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, maxDelay.TotalSeconds));
            }
        }

        _isReconnecting = false;
    }
}
