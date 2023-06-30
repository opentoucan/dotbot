namespace Dotbot.Models;

public class BotCommand : Entity
{
    public string ServiceId { get; set; }
    public string Name { get; set; }

    public string Content { get; set; }
    
    public BotCommandType Type { get; set; }
    
    public string? CreatorId { get; set; }
    
    public DateTimeOffset Created { get; set; }

    public BotCommand(string serviceId, string name, string content, BotCommandType botCommandType, string creatorId)
    {
        ServiceId = serviceId;
        Name = name;
        Content = content;
        Type = botCommandType;
        CreatorId = creatorId;
        Created = DateTimeOffset.UtcNow;
    }
    
}