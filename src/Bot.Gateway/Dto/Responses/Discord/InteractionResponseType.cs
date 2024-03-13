namespace Bot.Gateway.Dto.Responses.Discord;

public enum InteractionResponseType
{
    Ping = 1,
    ChannelMessageWithSource = 4,
    DeferredInteractionResponse = 5,
    AutocompleteResponse = 8
}