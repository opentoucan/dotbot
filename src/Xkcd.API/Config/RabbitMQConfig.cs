namespace Xkcd.API.Config;

public class RabbitMQConfig
{
    public string Endpoint { get; set; } = null!;
    public string Port { get; set; } = null!;
    public string User { get; set; } = null!;
    public string Password { get; set; } = null!;
}