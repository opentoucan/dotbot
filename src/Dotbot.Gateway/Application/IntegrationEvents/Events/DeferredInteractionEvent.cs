using Dotbot.Gateway.Dto.Requests.Discord;

// ReSharper disable once CheckNamespace
namespace Contracts.MessageBus;

public class DeferredInteractionEvent
{
    public InteractionRequest Request { get; set; }
    
    public DeferredInteractionEvent(InteractionRequest request)
    {
        Request = request;
    }
}