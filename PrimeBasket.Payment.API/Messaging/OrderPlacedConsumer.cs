using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using PrimeBasket.Common.Messaging;
using PrimeBasket.Common.Events;

namespace PrimeBasket.Payment.API.Messaging;

public class OrderPlacedConsumer : BackgroundService
{
    private readonly ILogger<OrderPlacedConsumer> _logger;
    private readonly RabbitMQSettings _settings;
    private IConnection _connection;
    private IModel _channel;

    public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger, IOptions<RabbitMQSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
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

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: _settings.ExchangeName, type: ExchangeType.Direct);
        _channel.QueueDeclare(queue: "payment.orderplaced.queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(queue: "payment.orderplaced.queue", exchange: _settings.ExchangeName, routingKey: "order.placed");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => _logger.LogInformation("RabbitMQ consumer is stopping."));

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var orderPlacedEvent = JsonSerializer.Deserialize<OrderPlacedEvent>(message);

            _logger.LogInformation($"[RabbitMQ] Received OrderPlacedEvent: OrderId={orderPlacedEvent?.OrderId}, UserId={orderPlacedEvent?.UserId}, Amount={orderPlacedEvent?.TotalAmount}");

            // Here you can process the payment or trigger other logic
            
            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        };

        _channel.BasicConsume(queue: "payment.orderplaced.queue", autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
