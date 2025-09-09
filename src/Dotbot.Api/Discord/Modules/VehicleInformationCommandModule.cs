using System.Diagnostics;
using System.Text.RegularExpressions;
using Dotbot.Api.Dto;
using Dotbot.Api.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;
using ServiceDefaults;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Dotbot.Api.Discord.Modules;

public class StringMenuModule(IRedisDatabase redis, ILogger<StringMenuModule> logger)
    : ComponentInteractionModule<HttpStringMenuInteractionContext>
{
    [ComponentInteraction("mot_menu")]
    public async Task Menu()
    {
        var selectedValue = Context.SelectedValues.ToList().FirstOrDefault();
        var registrationPlate = selectedValue?.Split("-")[0];
        var motTestNumber = selectedValue?.Split("-")[1];
        VehicleInformationAggregate? vehicleInformation;

        try
        {
            vehicleInformation = await redis.GetAsync<VehicleInformationAggregate>(registrationPlate!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting MOT test for string menu select option {regAndMotTestNumber}",
                selectedValue);
            return;
        }

        var motTest = vehicleInformation?.VehicleMotTests.FirstOrDefault(x => x.TestNumber == motTestNumber);
        var motTestEmbed = VehicleInformationAndMotEmbedBuilder.BuildMotTestEmbed(registrationPlate, motTest!);
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.ModifyMessage(action => { action.WithEmbeds([motTestEmbed]); }),
            cancellationToken: CancellationToken.None);
    }
}

public class ButtonModule(IRedisDatabase redis, ILogger<ButtonModule> logger)
    : ComponentInteractionModule<HttpButtonInteractionContext>
{
    [ComponentInteraction("mot_button")]
    public async Task Button()
    {
        var interactionResponse = new InteractionMessageProperties();

        VehicleInformationAggregate? vehicleInformation;
        try
        {
            vehicleInformation =
                await redis.GetAsync<VehicleInformationAggregate>(Context.Interaction.Message.Id.ToString());
            if (vehicleInformation == null)
            {
                await RespondAsync(InteractionCallback.Message(
                    new InteractionMessageProperties().WithContent(
                            "This is message is too old to use, please perform another slash command for this")
                        .WithFlags(MessageFlags.Ephemeral)));
                return;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error getting vehicle vehicle information from message ID {messageId} from the cache, unable to load mot history select menu",
                Context.Interaction.Message.Id.ToString());
            await RespondAsync(InteractionCallback.Message(
                new InteractionMessageProperties().WithContent(
                        "Something has gone wrong using this interaction. Please try again later")
                    .WithFlags(MessageFlags.Ephemeral)));
            return;
        }

        var stringMenu = new StringMenuProperties("mot_menu");

        foreach (var motTest in vehicleInformation.VehicleMotTests.OrderByDescending(motTest => motTest.CompletedDate)
                     .Take(10))
            stringMenu.Add(new StringMenuSelectOptionProperties(
                $"{vehicleInformation.VehicleMotTests.IndexOf(motTest) + 1} - {motTest.CompletedDate.GetValueOrDefault().ToShortDateString()} - {motTest.Result} - {motTest.OdometerReadingInMiles} Miles",
                $"{vehicleInformation.Registration}-{motTest.TestNumber}"));

        interactionResponse
            .WithContent("Select an MOT from the drop down")
            .AddComponents(stringMenu)
            .WithFlags(MessageFlags.Ephemeral);

        await RespondAsync(InteractionCallback.Message(interactionResponse));
    }
}

[SlashCommand("vehicle", "Retrieves vehicle information and MOT history")]
public partial class VehicleInformationCommandModule(
    IMoturService moturService,
    IVehicleInformationService vehicleInformationService,
    ILogger<VehicleInformationCommandModule> logger,
    IRedisDatabase redis) : ApplicationCommandModule<HttpApplicationCommandContext>
{
    [SubSlashCommand("registration", "Registration plate to search")]
    public async Task RetrieveByRegistration(
        [SlashCommandParameter(Name = "registration", Description = "Registration plate")]
        string reg)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        using var activity = Instrumentation.ActivitySource.StartActivity(ActivityKind.Client);
        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(),
            cancellationToken: cancellationTokenSource.Token);
        var normalisedRegistrationPlate = AllWhitespaceRegex().Replace(reg, "").ToUpper();

        await RetrieveAndPostVehicleData(normalisedRegistrationPlate, activity, "vehicle_registration",
            cancellationTokenSource.Token);
    }

    [SubSlashCommand("link", "URL of a vehicle advert i.e. autotrader or carandclassic ad")]
    public async Task RetrieveByLink(
        [SlashCommandParameter(Name = "link", Description = "URL")]
        string link)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        using var activity = Instrumentation.ActivitySource.StartActivity(ActivityKind.Client);
        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(),
            cancellationToken: cancellationTokenSource.Token);
        try
        {
            var registrationPlateResult =
                await moturService.GetRegistrationPlateByVehicleAdvert(link, cancellationTokenSource.Token);
            if (!registrationPlateResult.IsSuccess)
                await Context.Interaction.SendFollowupMessageAsync(
                    registrationPlateResult.ErrorResult?.ErrorMessage!,
                    cancellationToken: cancellationTokenSource.Token);

            await RetrieveAndPostVehicleData(registrationPlateResult.Value!, activity, "vehicle_link",
                cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to communicate with Motur: {exception}", ex.Message);
            await Context.Interaction.SendFollowupMessageAsync(
                "An error occurred trying to identify the registration plate",
                cancellationToken: cancellationTokenSource.Token);
        }
    }

    private async Task RetrieveAndPostVehicleData(string registrationPlate, Activity? activity, string commandName,
        CancellationToken cancellationToken)
    {
        var displayName = (Context.Interaction.User as GuildUser)?.Nickname ??
                          Context.Interaction.User.GlobalName ?? Context.Interaction.User.Username;
        var tags = new TagList
        {
            { "interaction_name", commandName },
            { "member_id", Context.Interaction.User.Id },
            { "member_display_name", displayName }
        };

        var vehicleInformationResult =
            await vehicleInformationService.GetVehicleInformation(registrationPlate, cancellationToken);

        var vehicleSummaryEmbed = !vehicleInformationResult.IsSuccess
            ? new EmbedProperties()
                .WithTitle($"No Vehicle Information found for {registrationPlate}")
                .WithColor(new Color(255, 0, 0))
            : VehicleInformationAndMotEmbedBuilder.BuildVehicleInformationEmbed(vehicleInformationResult.Value!);

        var motHistoryEmbed =
            VehicleInformationAndMotEmbedBuilder.BuildMotSummaryEmbed(vehicleInformationResult.Value!);

        var linkButtonComponents = new List<ActionRowProperties>
        {
            new ActionRowProperties().AddComponents(new LinkButtonProperties(
                    $"https://www.check-mot.service.gov.uk/results?registration={vehicleInformationResult.Value!.Registration}",
                    "Link to full MOT History"),
                new ButtonProperties("mot_button", "Show MOT history in Discord", ButtonStyle.Primary))
        };
        var interactionResponse = new InteractionMessageProperties().WithEmbeds([vehicleSummaryEmbed]);

        if (vehicleInformationResult.Value.VehicleMotTests.Count > 0)
            interactionResponse
                .AddEmbeds(motHistoryEmbed)
                .AddComponents(linkButtonComponents);

        var message =
            await Context.Interaction.SendFollowupMessageAsync(interactionResponse,
                cancellationToken: cancellationToken);

        await redis.AddAsync(message.Id.ToString(), vehicleInformationResult.Value, TimeSpan.FromMinutes(30));

        tags.Add(new KeyValuePair<string, object?>("registration", vehicleInformationResult.Value.Registration));
        tags.Add(new KeyValuePair<string, object?>("make", vehicleInformationResult.Value.Make));
        tags.Add(new KeyValuePair<string, object?>("model", vehicleInformationResult.Value.Model));
        tags.Add(new KeyValuePair<string, object?>("colour", vehicleInformationResult.Value.Colour));
        tags.Add(new KeyValuePair<string, object?>("fuel_type", vehicleInformationResult.Value.FuelType));
        tags.Add(new KeyValuePair<string, object?>("engine_size", vehicleInformationResult.Value.EngineCapacityLitres));
        foreach (var tag in tags)
            activity?.SetTag(tag.Key, tag.Value);

        Instrumentation.VehicleRegistrationCounter.Add(1, tags);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex AllWhitespaceRegex();
}