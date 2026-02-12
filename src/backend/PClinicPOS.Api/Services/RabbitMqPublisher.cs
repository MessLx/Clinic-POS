using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace PClinicPOS.Api.Services;

public class RabbitMqPublisher : IMessagePublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string ExchangeName = "peaaura.events";

    public RabbitMqPublisher(string connectionString)
    {
        var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
    }

    public void Publish(string eventName, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var body = Encoding.UTF8.GetBytes(json);
        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";
        _channel.BasicPublish(ExchangeName, eventName, props, body);
    }
}
