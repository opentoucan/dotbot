using System.Text.Json;
using System.Text.Json.Serialization;
using Contracts.MessageBus;
using Discord;
using Dotbot.Gateway.Dto.Requests.Discord;
using Dotbot.Gateway.Dto.Responses.Discord;
using Dotbot.Gateway.Extensions;
using Microsoft.AspNetCore.Mvc;
using InteractionResponseType = Dotbot.Gateway.Dto.Responses.Discord.InteractionResponseType;

namespace Dotbot.Gateway.Apis;

public static class DiscordInteractionApi
{
    public static RouteGroupBuilder MapDiscordInteractionApi(this RouteGroupBuilder app)
    {
        app.MapPost("/", Interaction);
        return app;
    }
    
    public static async Task<IResult> Interaction([AsParameters] DiscordInteractionService service, [FromBody]InteractionRequest request, CancellationToken cancellationToken)
    {
        var serializerSettings = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        
        //Discord health check
        if (request.Type == (int)InteractionType.Ping)
            return Results.Json(new InteractionResponse { Type = InteractionResponseType.Ping }, serializerSettings);

        var guildId = request.Guild?.Id;
        
        if(string.IsNullOrEmpty(guildId))
            return TypedResults.BadRequest("The request must be made in the context of a server");

        var interactionType = DiscordExtensions.GetInteractionCommands()
            .FirstOrDefault(x => x.Name.Value == request.Data?.Name)
            ?.Name.Value;
        if(string.IsNullOrWhiteSpace(interactionType))
            return TypedResults.NotFound($"Command not found for {request.Data?.Name!}");
        
        
        //If the interaction is an autocomplete request then handle it here
        if (request.Type == (int)InteractionType.ApplicationCommandAutocomplete && interactionType == "custom")
        {
            var commandJson = (JsonElement?)request.Data!.Options!.FirstOrDefault()?.Value;
            //Might want to use a fuzzy search
            var customCommands = await service.Queries.GetCustomCommandsByFuzzySearchOnNameAsync(guildId, commandJson?.GetString()!);
            
            return Results.Json(new InteractionResponse
            {
                Type = InteractionResponseType.AutocompleteResponse,
                Data = new InteractionData(choices: customCommands.Select(bc => new Choice { Name = bc.Name, Value = bc.Name }).ToList())
            }, serializerSettings);
        }

        await service.Scheduler.SchedulePublish(DateTime.UtcNow, new DeferredInteractionEvent(request), cancellationToken);
        return Results.Json(new InteractionResponse{ Type = InteractionResponseType.DeferredInteractionResponse}, serializerSettings);
    }
}

