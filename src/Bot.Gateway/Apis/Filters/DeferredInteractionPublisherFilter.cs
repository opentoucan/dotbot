using System.Text.Json;
using Bot.Gateway.Dto.Requests.Discord;
using Bot.Gateway.Dto.Responses.Discord;
using Contracts.MessageBus;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Bot.Gateway.Apis.Filters;

public class DeferredInteractionPublisherFilter : IEndpointFilter
{
    private readonly IBus _bus;

    public DeferredInteractionPublisherFilter(IBus bus)
    {
        _bus = bus;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);
        var response = result as JsonHttpResult<InteractionResponse>;
        if (response?.Value?.Type == InteractionResponseType.DeferredInteractionResponse)
        {
            var body = context.HttpContext.Request.Body;
            body.Seek(0, SeekOrigin.Begin);

            using var bodyReader = new StreamReader(body);
            string rawBody = await bodyReader.ReadToEndAsync();
            var interactionRequest = JsonSerializer.Deserialize<InteractionRequest>(rawBody);
            await _bus.Publish(new DeferredInteractionEvent(interactionRequest!));
        }

        return result;
    }
}