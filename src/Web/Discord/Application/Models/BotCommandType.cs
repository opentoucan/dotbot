using Discord.SeedWork;

namespace Discord.Application.Models;

public class BotCommandType : Enumeration
{
    public static BotCommandType String = new BotCommandType(0, nameof(String));
    public static BotCommandType File = new BotCommandType(1, nameof(File));
    
    public BotCommandType(int id, string name) : base(id, name) { }
}