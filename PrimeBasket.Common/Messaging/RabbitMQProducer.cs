using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace PrimeBasket.Common.Messaging;

public interface IMessageProducer
{
    void SendMessage<T>(T message, string routingKey);
}

public class RabbitMQProducer : IMessageProducer
{
    private readonly RabbitMQSettings _settings;

    public RabbitMQProducer(IOptions<RabbitMQSettings> settings)
    {
        _settings = settings.Value;
    }

    public void SendMessage<T>(T message, string routingKey)
    {
        var factory = new ConnectionFactory();
        if (_settings.Hostname.StartsWith("amqp"))
        {
            factory.Uri = new Uri(_settings.Hostname);
        }
        else
        {
            factory.HostName = _settings.Hostname;
            factory.UserName = _settings.Username;
            factory.Password = _settings.Password;
        }

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(exchange: _settings.ExchangeName, type: ExchangeType.Direct);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        channel.BasicPublish(exchange: _settings.ExchangeName,
                             routingKey: routingKey,
                             basicProperties: null,
                             body: body);
    }
}
