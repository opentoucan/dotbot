namespace Dotbot.Infrastructure.Entities;

public class CommandAttachment : Entity
{
    public string Name { get; private set; } = null!;
    public string FileType { get; private set; } = null!;
    public string Url { get; private set; } = null!;

    protected CommandAttachment() { }
    public CommandAttachment(string name, string fileType, string url)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException($"{nameof(name)}", "Name cannot be blank");
        if (string.IsNullOrWhiteSpace(fileType))
            throw new ArgumentNullException($"{nameof(fileType)}", "File type cannot be empty");
        Name = name;
        FileType = fileType;
        Url = url;
    }
}