namespace BtcTrader.Infrastructure.RabbitMq;

public class RabbitMqSettings
{
    public RabbitMqSettings(string uri, string userName, string password)
    {
        Uri = uri;
        UserName = userName;
        Password = password;
    }

    public string Uri { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
}
