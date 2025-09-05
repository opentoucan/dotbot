using System.Diagnostics;
using System.Text.RegularExpressions;
using Dotbot.Api.Dto.MotApi;
using Dotbot.Api.Dto.VesApi;
using Dotbot.Api.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using ServiceDefaults;

namespace Dotbot.Api.Discord.Modules;

[SlashCommand("vehicle", "Retrieves vehicle information and MOT history")]
public class VehicleInformationCommandModule(
    IMoturService moturService,
    IVehicleEnquiryService vehicleEnquiryService,
    IMotHistoryService motHistoryService,
    Instrumentation instrumentation,
    ILogger<VehicleInformationCommandModule> logger) : ApplicationCommandModule<HttpApplicationCommandContext>
{
    [SubSlashCommand("registration", "Registration plate to search")]
    public async Task RetrieveByRegistration(
        [SlashCommandParameter(Name = "registration", Description = "Registration plate")]
        string reg)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        using var activity = instrumentation.ActivitySource.StartActivity(ActivityKind.Client);
        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(),
            cancellationToken: cancellationTokenSource.Token);
        var normalisedRegistrationPlate = Regex.Replace(reg, @"\s+", "").ToUpper();

        await RetrieveAndPostVehicleData(normalisedRegistrationPlate, activity, "vehicle_registration",
            cancellationTokenSource.Token);
    }

    [SubSlashCommand("link", "URL of a vehicle advert i.e. autotrader or carandclassic ad")]
    public async Task RetrieveByLink(
        [SlashCommandParameter(Name = "link", Description = "URL")]
        string link)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        using var activity = instrumentation.ActivitySource.StartActivity(ActivityKind.Client);
        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(),
            cancellationToken: cancellationTokenSource.Token);
        try
        {
            var registrationPlateResult =
                await moturService.GetRegistrationPlateByVehicleAdvert(link, cancellationTokenSource.Token);
            if (!registrationPlateResult.IsSuccess)
                await Context.Interaction.SendFollowupMessageAsync(
                    registrationPlateResult.Errors.FirstOrDefault()!,
                    cancellationToken: cancellationTokenSource.Token);

            await RetrieveAndPostVehicleData(registrationPlateResult, activity, "vehicle_link",
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
        var commonTags = new TagList
        {
            { "interaction_name", commandName },
            { "member_id", Context.Interaction.User.Id },
            { "member_display_name", displayName }
        };

        var vehicleRegistrationResult =
            await vehicleEnquiryService.GetVehicleRegistrationInformation(registrationPlate, cancellationToken);
        if (!vehicleRegistrationResult.IsSuccess)
        {
            if (vehicleRegistrationResult.StatusCode != null)
                instrumentation.VesApiErrorCounter.Add(1,
                    new KeyValuePair<string, object?>("ves_api_error",
                        (int)vehicleRegistrationResult.StatusCode.Value));

            foreach (var tag in instrumentation.VesApiErrorCounter.Tags ?? [])
                activity?.SetTag(tag.Key, tag.Value);

            instrumentation.VesApiErrorCounter.Add(1, commonTags);


            if (vehicleRegistrationResult.Exception != null)
            {
                instrumentation.ExceptionCounter.Add(1, commonTags);
                instrumentation.ExceptionCounter.Add(1,
                    new KeyValuePair<string, object?>("type", vehicleRegistrationResult.Exception),
                    new KeyValuePair<string, object?>("interaction_name", commandName));
                foreach (var tag in instrumentation.ExceptionCounter.Tags!)
                    activity?.SetTag(tag.Key, tag.Value);
            }
        }

        var motHistoryResult = await motHistoryService.GetVehicleMotHistory(registrationPlate, cancellationToken);

        if (!motHistoryResult.IsSuccess)
        {
            if (vehicleRegistrationResult.StatusCode != null)
                instrumentation.MotApiErrorCounter.Add(1,
                    new KeyValuePair<string, object?>("mot_api_error",
                        (int)vehicleRegistrationResult.StatusCode.Value));

            foreach (var tag in instrumentation.MotApiErrorCounter.Tags ?? [])
                activity?.SetTag(tag.Key, tag.Value);

            instrumentation.MotApiErrorCounter.Add(1, commonTags);


            if (vehicleRegistrationResult.Exception != null)
            {
                instrumentation.ExceptionCounter.Add(1, commonTags);
                instrumentation.ExceptionCounter.Add(1,
                    new KeyValuePair<string, object?>("type", vehicleRegistrationResult.Exception),
                    new KeyValuePair<string, object?>("interaction_name", commandName));
                foreach (var tag in instrumentation.ExceptionCounter.Tags!)
                    activity?.SetTag(tag.Key, tag.Value);
            }
        }

        await SendVehicleInformationEmbedAsync(vehicleRegistrationResult, motHistoryResult);

        foreach (var tag in commonTags)
            activity?.SetTag(tag.Key, tag.Value);

        instrumentation.VehicleRegistrationCounter.Add(1, commonTags);
    }

    private async Task SendVehicleInformationEmbedAsync(ServiceResult<VesApiResponse> vehicleRegistrationResult,
        ServiceResult<MotApiResponse> motHistoryResult)
    {
        var embeds = new List<EmbedProperties>();
        var vehicleSummaryEmbed =
            VehicleInformationAndMotEmbedBuilder.BuildVehicleInformationEmbed(vehicleRegistrationResult,
                motHistoryResult);

        embeds.AddRange(vehicleSummaryEmbed,
            VehicleInformationAndMotEmbedBuilder.BuildMotSummaryEmbed(motHistoryResult));

        var linkButtonComponents = new List<ActionRowProperties>
        {
            new ActionRowProperties().AddComponents(new LinkButtonProperties(
                $"https://www.check-mot.service.gov.uk/results?registration={motHistoryResult.Value?.Registration}",
                "Link to full MOT History"))
        };
        var interactionResponse = new InteractionMessageProperties().WithEmbeds(embeds);

        if (motHistoryResult.IsSuccess && motHistoryResult.Value?.MotTests.Count > 0)
            interactionResponse.WithComponents(linkButtonComponents);

        await Context.Interaction.SendFollowupMessageAsync(interactionResponse);
    }
}