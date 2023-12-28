namespace Bot.Gateway.Model.Responses.Discord;

public class InteractionResponse
{
    public InteractionResponseType Type { get; set; }
    
    public InteractionData Data { get; set; } = new();
}