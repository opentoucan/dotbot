namespace Bot.Gateway.Infrastructure.Entities;

public class BotCommand : Entity
{
    public string ServerId { get; set; }
    public string Name { get; set; }

    public string? Content { get; set; }
    
    public List<string>? AttachmentIds { get; set; }
    
    public string? CreatorId { get; set; }
    
    public DateTimeOffset Created { get; set; }

    public BotCommand(string serverId, string name, string creatorId, string? content = null, List<string>? attachmentIds = null)
    {
        ServerId = serverId;
        Name = name;
        Content = content;
        CreatorId = creatorId;
        Created = DateTimeOffset.UtcNow;
        AttachmentIds = attachmentIds;
    }

}