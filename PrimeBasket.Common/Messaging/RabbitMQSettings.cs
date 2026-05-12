namespace PrimeBasket.Common.Messaging;

public class RabbitMQSettings
{
    public string Hostname { get; set; } = "localhost";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "PrimeBasketExchange";
}
