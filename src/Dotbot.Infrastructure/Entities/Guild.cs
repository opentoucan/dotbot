using Dotbot.Infrastructure.SeedWork;

namespace Dotbot.Infrastructure.Entities;

public class Guild : Entity
{
    public string ExternalId { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    public List<CustomCommand> CustomCommands { get; private set; } = [];

    protected Guild() { }
    public Guild(string externalId, string name)
    {
        ExternalId = externalId;
        Name = name;
    }

    public void SetName(string name)
    {
        Name = name;
    }

    public CustomCommand AddCustomCommand(string name, string creatorId, string? content = null)
    {
        var customCommand = new CustomCommand(name, creatorId, content);
        CustomCommands.Add(customCommand);
        return customCommand;
    }
}