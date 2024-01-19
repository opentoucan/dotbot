using Discord;

namespace Bot.Gateway.Model.Responses.Discord;

public class InteractionData
{
    public InteractionData(
        string? content = null, 
        List<Embed>? embeds = null, 
        List<DiscordFileAttachment>? fileAttachments = null,
        List<Choice>? choices = null)
    {
        Content = content;
        Embeds = embeds ?? [];
        FileAttachments = fileAttachments ?? [];
        Choices = choices;
    }
    public string? Content { get; set; }
    public List<Embed> Embeds { get; set; }
    public List<DiscordFileAttachment> FileAttachments { get; set; }
    public List<Choice>? Choices { get; set; }
}