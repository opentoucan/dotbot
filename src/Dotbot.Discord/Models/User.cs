namespace Dotbot.Discord.Models;

public class User
{
    public ulong Id { get; init; }
    public string EffectiveAvatarUrl { get; init; }
    public string Username { get; set; }
    public string Nickname { get; set; }
}