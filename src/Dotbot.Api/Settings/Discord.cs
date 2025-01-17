namespace Dotbot.Api.Settings;

public class Discord
{
    public string Token { get; init; } = null!;
    public string BucketEnvPrefix { get; set; } = null!;
}