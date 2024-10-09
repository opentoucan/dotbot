namespace Dotbot.Gateway.Dto.Responses.Discord;

public class DiscordFileAttachment
{
    public DiscordFileAttachment(string filename, string description, bool isSpoiler, byte[] fileContent)
    {
        Filename = filename;
        Description = description;
        IsSpoiler = isSpoiler;
        FileContent = fileContent;
    }

    public string Filename { get; set; }
    
    public string Description { get; set; }
    
    public bool IsSpoiler { get; private set; }
    
    public byte[] FileContent { get; set; }
    
}