using System.Text.Json;
using System.Text.Json.Serialization;
using Bot.Gateway.Apis.Filters;
using Bot.Gateway.Dto.Requests.Discord;
using Bot.Gateway.Dto.Responses.Discord;
using Bot.Gateway.Extensions;
using Discord;
using Microsoft.AspNetCore.Mvc;
using InteractionResponseType = Bot.Gateway.Dto.Responses.Discord.InteractionResponseType;

namespace Bot.Gateway.Apis;

public static class DiscordInteractionApi
{
    public static RouteGroupBuilder MapDiscordInteractionApi(this RouteGroupBuilder app)
    {
        app.MapPost("/", Interaction).AddEndpointFilter<DeferredInteractionPublisherFilter>();
        return app;
    }
    
    public static IResult Interaction([AsParameters] DiscordInteractionService service, [FromBody]InteractionRequest request, CancellationToken cancellationToken)
    {
        var serializerSettings = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        
        //Discord health check
        if (request.Type == (int)InteractionType.Ping)
            return Results.Json(new InteractionResponse { Type = InteractionResponseType.Ping }, serializerSettings);
        
        var interactionType = DiscordExtensions.GetInteractionCommands()
            .FirstOrDefault(x => x.Name.Value == request.Data?.Name)
            ?.Name.Value;
        if(string.IsNullOrWhiteSpace(interactionType))
            return TypedResults.NotFound($"Command not found for {request.Data?.Name!}");
        
        
        //If the interaction is an autocomplete request then handle it here
        if (request.Type == (int)InteractionType.ApplicationCommandAutocomplete && interactionType == "custom")
        {
            //This needs refactoring into some kind of polymorphic handler
            var commandJson = (JsonElement?)request.Data!.Options!.FirstOrDefault()?.Value;
            var botCommands = service.BotCommandRepository.SearchByNameAndServer(request.Data!.GuildId!, commandJson?.GetString(), 10);
            
            //The choices should use an automapper
            return Results.Json(new InteractionResponse
            {
                Type = InteractionResponseType.AutocompleteResponse,
                Data = new InteractionData(choices: botCommands.Select(bc => new Choice { Name = bc.Name, Value = bc.Name }).ToList())
            }, serializerSettings);
        }

        return Results.Json(new InteractionResponse{ Type = InteractionResponseType.DeferredInteractionResponse}, serializerSettings);
    }
}

