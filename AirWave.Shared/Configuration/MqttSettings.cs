namespace AirWave.Shared.Configuration;

public class MqttSettings
{
    public string BrokerHost { get; set; } = "broker.hivemq.com";
    public int BrokerPort { get; set; } = 1883;
    public string Topic { get; set; } = "airwave/aqi";
    public int PublishIntervalSeconds { get; set; } = 20;
}
