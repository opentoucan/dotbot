using System.Text.Json;
using Contracts.MessageBus;
using Bot.Gateway.Dto.Requests.Discord;
using Bot.Gateway.Dto.Responses.Discord;
using MassTransit;
using MassTransit.Scheduling;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Bot.Gateway.Apis.Filters;

public class DeferredInteractionPublisherFilter(IBus bus) : IEndpointFilter
{
    private readonly MessageScheduler _scheduler = new(new DelayedScheduleMessageProvider(bus), bus.Topology as IRabbitMqBusTopology);

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);
        var response = result as JsonHttpResult<InteractionResponse>;
        if (response?.Value?.Type == InteractionResponseType.DeferredInteractionResponse)
        {
            using var memoryStream = new MemoryStream();
            context.HttpContext.Request.Body.CopyTo(memoryStream);
            var body = context.HttpContext.Request.Body;
            body.Seek(0, SeekOrigin.Begin);

            using var bodyReader = new StreamReader(body);
            var rawBody = await bodyReader.ReadToEndAsync();
            var interactionRequest = JsonSerializer.Deserialize<InteractionRequest>(rawBody);
            
            await _scheduler.SchedulePublish(DateTime.UtcNow + TimeSpan.FromSeconds(2), new DeferredInteractionEvent(interactionRequest!));
        }
        return result;
    }
}