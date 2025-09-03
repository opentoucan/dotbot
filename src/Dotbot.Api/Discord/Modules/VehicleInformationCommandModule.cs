using System.Diagnostics;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Ardalis.Result;
using Dotbot.Api.Dto;
using Dotbot.Api.Dto.MotApi;
using Dotbot.Api.Dto.VesApi;
using Dotbot.Api.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using ServiceDefaults;

namespace Dotbot.Api.Discord.Modules;

[SlashCommand("vehicle", "Retrieves vehicle information and MOT history")]
public class VehicleInformationCommandModule(IMoturService moturService, IVehicleEnquiryService vehicleEnquiryService, IMotHistoryService motHistoryService, Instrumentation instrumentation, ILogger<VehicleInformationCommandModule> logger) : ApplicationCommandModule<HttpApplicationCommandContext>
{
    [SubSlashCommand("registration", "Registration plate to search")]
    public async Task RetrieveByRegistration(
        [SlashCommandParameter(Name = "registration", Description = "Registration plate")]
        string reg)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        using var activity = instrumentation.ActivitySource.StartActivity(ActivityKind.Client);
        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(), cancellationToken: cancellationTokenSource.Token);
        var normalisedRegistrationPlate = Regex.Replace(reg, @"\s+", "").ToUpper();
        
        var vehicleRegistrationResult = await vehicleEnquiryService.GetVehicleRegistrationInformation(normalisedRegistrationPlate, cancellationTokenSource.Token);
        var motHistoryResult = await motHistoryService.GetVehicleMotHistory(normalisedRegistrationPlate, cancellationTokenSource.Token);

        await SendVehicleInformationEmbedAsync(vehicleRegistrationResult, motHistoryResult);
        
        var displayName = (Context.Interaction.User as GuildUser)?.Nickname ?? Context.Interaction.User.GlobalName ?? Context.Interaction.User.Username;

        var tags = new TagList
        {
            { "command_name", "vehicle_registration" },
            { "member_id", Context.Interaction.User.Id },
            { "member_display_name", displayName }
        };

        foreach (var tag in tags)
            activity?.SetTag(tag.Key, tag.Value);

        instrumentation.VehicleRegistrationCounter.Add(1, tags);
    }

    [SubSlashCommand("link", "URL of a vehicle advert i.e. autotrader or carandclassic ad")]
    public async Task RetrieveByLink(
        [SlashCommandParameter(Name = "link", Description = "URL")]
        string link)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        using var activity = instrumentation.ActivitySource.StartActivity(ActivityKind.Client);
        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(), cancellationToken: cancellationTokenSource.Token);
        try
        {
            var registrationPlateResult =
                await moturService.GetRegistrationPlateByVehicleAdvert(link, cancellationTokenSource.Token);
            if (!registrationPlateResult.IsSuccess)
                await Context.Interaction.SendFollowupMessageAsync(
                    registrationPlateResult.Errors.FirstOrDefault()!,
                    cancellationToken: cancellationTokenSource.Token);

            var vehicleRegistrationResult =
                await vehicleEnquiryService.GetVehicleRegistrationInformation(registrationPlateResult.Value,
                    cancellationTokenSource.Token);
            var motHistoryResult =
                await motHistoryService.GetVehicleMotHistory(registrationPlateResult.Value,
                    cancellationTokenSource.Token);

            await SendVehicleInformationEmbedAsync(vehicleRegistrationResult, motHistoryResult);
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to communicate with Motur: {exception}", ex.Message);
            await Context.Interaction.SendFollowupMessageAsync("An error occurred trying to identify the registration plate", cancellationToken: cancellationTokenSource.Token);
        }

        var displayName = (Context.Interaction.User as GuildUser)?.Nickname ?? Context.Interaction.User.GlobalName ?? Context.Interaction.User.Username;

        var tags = new TagList
        {
            { "command_name", "vehicle_link" },
            { "member_id", Context.Interaction.User.Id },
            { "member_display_name", displayName }
        };

        foreach (var tag in tags)
            activity?.SetTag(tag.Key, tag.Value);

        instrumentation.VehicleLinkCounter.Add(1, tags);
    }

    private async Task SendVehicleInformationEmbedAsync(Result<VesApiResponse> vehicleRegistrationResult, Result<MotApiResponse> motHistoryResult)
    {
        var embeds = new List<EmbedProperties>();
        var vehicleSummaryEmbed = VehicleInformationAndMotEmbedBuilder.BuildVehicleInformationEmbed(vehicleRegistrationResult, motHistoryResult);

        embeds.AddRange(vehicleSummaryEmbed, VehicleInformationAndMotEmbedBuilder.BuildMotSummaryEmbed(motHistoryResult));

        var linkButtonComponents = new List<ActionRowProperties>
        {
            new ActionRowProperties().AddComponents(new LinkButtonProperties(
                $"https://www.check-mot.service.gov.uk/results?registration={motHistoryResult.Value?.Registration}",
                label: "Link to full MOT History"))
        };
        var interactionResponse = new InteractionMessageProperties().WithEmbeds(embeds);

        if (motHistoryResult.IsSuccess && motHistoryResult.Value != null)
            interactionResponse.WithComponents(linkButtonComponents);
        
        await Context.Interaction.SendFollowupMessageAsync(interactionResponse);
    }
}
