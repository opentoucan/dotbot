using Contracts.MessageBus;
using Dotbot.Gateway.Application.InteractionCommands;
using Dotbot.Gateway.Application.InteractionCommands.Exceptions;
using Dotbot.Gateway.Dto.Responses.Discord;
using Dotbot.Gateway.Extensions;
using Dotbot.Gateway.HttpClient;
using MassTransit;
using MediatR;

namespace Dotbot.Gateway.Application.IntegrationEvents.EventHandlers;

public class DeferredInteractionEventHandler(
    IMediator mediator, 
    IDiscordHttpRequestHelper discordHttpRequestHelper,
    IInteractionCommandFactory interactionCommandFactory, 
    ILogger<DeferredInteractionEventHandler> logger) 
    : IConsumer<DeferredInteractionEvent>
{
    public async Task Consume(ConsumeContext<DeferredInteractionEvent> context)
    {
        var interactionType = DiscordExtensions
            .GetInteractionCommands()
            .FirstOrDefault(x => x.Name.Value == context.Message.Request.Data?.Name)?
            .Name.Value!;

        try
        {
            var command = interactionCommandFactory.Create(interactionType, context.Message.Request);
            var response = await mediator.Send(command);
            await discordHttpRequestHelper
                .SendFollowupMessageAsync(
                    context.Message.Request.ApplicationId!,
                    context.Message.Request.Token!,
                    response,
                    context.CancellationToken);
        }
        catch (CommandValidationException validationException)
        {
            await discordHttpRequestHelper
                .SendFollowupMessageAsync(
                    context.Message.Request.ApplicationId!,
                    context.Message.Request.Token!,
                    new InteractionData(content: validationException.Message),
                    context.CancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError("Unhandled exception when handling interaction in {EventHandler} with error: {Exception}", nameof(DeferredInteractionEventHandler), exception.Message);
        }
    }
}