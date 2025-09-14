using System.Diagnostics;
using System.Text.RegularExpressions;
using Dotbot.Api.Services;
using Dotbot.Infrastructure;
using Dotbot.Infrastructure.Entities.Reports;
using Microsoft.EntityFrameworkCore;
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
        VehicleInformation? vehicleInformation;

        try
        {
            vehicleInformation = await redis.GetAsync<VehicleInformation>(registrationPlate!);
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

        VehicleInformation? vehicleInformation;
        try
        {
            vehicleInformation =
                await redis.GetAsync<VehicleInformation>(Context.Interaction.Message.Id.ToString());
            if (vehicleInformation is null)
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
                $"{vehicleInformation.VehicleMotTests.IndexOf(motTest) + 1} - {motTest.CompletedDate.GetValueOrDefault().DateTime.ToShortDateString()} - {motTest.Result} - {motTest.OdometerReadingInMiles} Miles",
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
    IRedisDatabase redis,
    DotbotContext dbContext) : ApplicationCommandModule<HttpApplicationCommandContext>
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
        var guildId = Context.Interaction.GuildId?.ToString();
        if (guildId is null)
        {
            await Context.Interaction.SendResponseAsync(
                InteractionCallback.Message("Cannot use this command outside of a server"),
                cancellationToken: cancellationToken);
            return;
        }

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

        EmbedProperties vehicleSummaryEmbed;
        EmbedProperties motHistoryEmbed;
        var interactionResponse = new InteractionMessageProperties();

        if (vehicleInformationResult.Value is null || !vehicleInformationResult.IsSuccess)
        {
            vehicleSummaryEmbed = new EmbedProperties()
                .WithTitle($"No Vehicle Information found for {registrationPlate}")
                .WithColor(new Color(255, 0, 0));
        }
        else
        {
            vehicleSummaryEmbed =
                VehicleInformationAndMotEmbedBuilder.BuildVehicleInformationEmbed(vehicleInformationResult.Value!);

            var existingVehicleInformation =
                await dbContext.VehicleInformation.FirstOrDefaultAsync(x => x.Registration == registrationPlate,
                    cancellationToken);
            if (existingVehicleInformation is null)
            {
                await dbContext.VehicleInformation.AddAsync(vehicleInformationResult.Value, cancellationToken);
            }
            else
            {
                vehicleInformationResult.Value.Id = existingVehicleInformation.Id;
                dbContext.Entry(existingVehicleInformation).CurrentValues.SetValues(vehicleInformationResult.Value);
            }

            await dbContext.VehicleCommandLogs.AddAsync(new VehicleCommandLog
            {
                RegistrationPlate = registrationPlate,
                RequestDate = DateTimeOffset.Now,
                GuildId = guildId,
                UserId = Context.Interaction.User.Id.ToString()
            }, cancellationToken);


            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (vehicleInformationResult.Value is not null && vehicleInformationResult.Value.VehicleMotTests.Count > 0)
        {
            motHistoryEmbed =
                VehicleInformationAndMotEmbedBuilder.BuildMotSummaryEmbed(vehicleInformationResult.Value);

            var linkButtonComponents = new List<ActionRowProperties>
            {
                new ActionRowProperties().AddComponents(new LinkButtonProperties(
                        $"https://www.check-mot.service.gov.uk/results?registration={vehicleInformationResult.Value!.Registration}",
                        "Link to full MOT History"),
                    new ButtonProperties("mot_button", "Show MOT history in Discord", ButtonStyle.Primary))
            };
            interactionResponse.AddComponents(linkButtonComponents);
        }
        else
        {
            motHistoryEmbed = new EmbedProperties()
                .WithDescription("No MOTs Found");
        }

        interactionResponse
            .AddEmbeds(vehicleSummaryEmbed)
            .AddEmbeds(motHistoryEmbed);

        var message =
            await Context.Interaction.SendFollowupMessageAsync(interactionResponse,
                cancellationToken: cancellationToken);

        if (vehicleInformationResult.Value is not null)
            await redis.AddAsync(message.Id.ToString(), vehicleInformationResult.Value, TimeSpan.FromMinutes(30));
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex AllWhitespaceRegex();
}