using System.ComponentModel.DataAnnotations;

namespace Dotbot.Infrastructure.Entities;

public class CustomCommand : Entity
{
    public string Name { get; private set; } = null!;
    public string? Content { get; private set; }
    public string CreatorId { get; private set; } = null!;
    public DateTimeOffset Created { get; private set; }

    public List<CommandAttachment> Attachments { get; private set; } = [];

    protected CustomCommand() { }
    public CustomCommand(string name, string creatorId, string? content = null)
    {
        Name = name;
        Content = content;
        CreatorId = creatorId;
        Created = DateTimeOffset.UtcNow;
    }

    public void AddAttachment(string name, string fileType, string url)
    {
        Attachments.Add(new CommandAttachment(name, fileType, url));
    }

    public void SetNewCommandContent(string? content, string creatorId)
    {
        Content = content;
        CreatorId = creatorId;
    }

    public void DeleteAllAttachments()
    {
        Attachments.Clear();
    }
}