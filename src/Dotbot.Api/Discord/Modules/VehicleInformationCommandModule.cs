using System.Diagnostics;
using System.Text.RegularExpressions;
using Dotbot.Api.Domain;
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
    ILogger<VehicleInformationCommandModule> logger) : ApplicationCommandModule<HttpApplicationCommandContext>
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

        var vehicleRegistrationResult =
            ServiceResult<VesApiResponse>.Error("Something went wrong fetching vehicle details");
        var motHistoryResult = ServiceResult<MotApiResponse>.Error("Something went wrong fetching MOT details");

        try
        {
            vehicleRegistrationResult =
                await vehicleEnquiryService.GetVehicleRegistrationInformation(registrationPlate, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unhandled error occurred when retrieving a response from the Vehicle Enquiry Service API");

            tags.Add(new KeyValuePair<string, object?>("type", ex.GetType().Name));
            tags.Add(new KeyValuePair<string, object?>("interaction_name", commandName));
            Instrumentation.ExceptionCounter.Add(1, tags);
        }

        try
        {
            motHistoryResult = await motHistoryService.GetVehicleMotHistory(registrationPlate, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error occurred when retrieving a response from the MOT History API");
            tags.Add(new KeyValuePair<string, object?>("type", ex.GetType().Name));
            tags.Add(new KeyValuePair<string, object?>("interaction_name", commandName));
            Instrumentation.ExceptionCounter.Add(1, tags);
        }

        VehicleInformation vehicleInformation;
        var registrationPlateResult = vehicleRegistrationResult.Value?.RegistrationNumber ??
                                      motHistoryResult.Value?.Registration;

        if ((!vehicleRegistrationResult.IsSuccess &&
             !motHistoryResult.IsSuccess) || string.IsNullOrWhiteSpace(registrationPlate))
        {
            await Context.Interaction.SendFollowupMessageAsync(
                $"{vehicleRegistrationResult.ErrorResult?.ErrorMessage}\n{motHistoryResult.ErrorResult?.ErrorMessage}",
                cancellationToken: cancellationToken);
            return;
        }

        var potentiallyScrapped = !vehicleRegistrationResult.IsSuccess && motHistoryResult.IsSuccess;

        vehicleInformation = new VehicleInformation(
            registrationPlateResult!,
            potentiallyScrapped,
            vehicleRegistrationResult.Value?.Make ?? motHistoryResult.Value?.Make,
            motHistoryResult.Value?.Model,
            vehicleRegistrationResult.Value?.Colour ?? motHistoryResult.Value?.PrimaryColour,
            vehicleRegistrationResult.Value?.FuelType ?? motHistoryResult.Value?.FuelType,
            vehicleRegistrationResult.Value?.MotStatus,
            motHistoryResult.Value?.MotTestDueDate,
            motHistoryResult.Value?.MotTests.Where(x => x.TestResult == "PASSED")
                .OrderByDescending(x => x.CompletedDate).FirstOrDefault()?.ExpiryDate,
            vehicleRegistrationResult.Value?.TaxStatus,
            vehicleRegistrationResult.Value?.TaxDueDate,
            !string.IsNullOrWhiteSpace(vehicleRegistrationResult.Value?.MonthOfFirstDvlaRegistration)
                ? DateTime.Parse($"01-{vehicleRegistrationResult.Value?.MonthOfFirstDvlaRegistration}")
                : motHistoryResult.Value?.RegistrationDate,
            vehicleRegistrationResult.Value?.EngineCapacity?.ToString() ?? motHistoryResult.Value?.EngineSize,
            vehicleRegistrationResult.Value?.RevenueWeight,
            vehicleRegistrationResult.Value?.Co2Emissions,
            vehicleRegistrationResult.Value?.DateOfLastV5cIssued);

        foreach (var motTest in motHistoryResult.Value?.MotTests ?? [])
            vehicleInformation.AddMotTest(
                motTest.TestResult,
                motTest.CompletedDate,
                motTest.OdometerValue,
                motTest.OdometerUnit,
                motTest.OdometerResultType,
                motTest.MotTestNumber,
                motTest.Defects.Select(defect =>
                    (defect.Type, defect.Text, defect.Dangerous)).ToList());

        var vehicleSummaryEmbed = !vehicleRegistrationResult.IsSuccess && !motHistoryResult.IsSuccess
            ? new EmbedProperties()
                .WithTitle($"No Vehicle Information found for {registrationPlate}")
                .WithColor(new Color(255, 0, 0))
            : VehicleInformationAndMotEmbedBuilder.BuildVehicleInformationEmbed(vehicleInformation);

        var motHistoryEmbed = VehicleInformationAndMotEmbedBuilder.BuildMotSummaryEmbed(vehicleInformation);

        var linkButtonComponents = new List<ActionRowProperties>
        {
            new ActionRowProperties().AddComponents(new LinkButtonProperties(
                $"https://www.check-mot.service.gov.uk/results?registration={vehicleInformation.Registration}",
                "Link to full MOT History"))
        };
        var interactionResponse = new InteractionMessageProperties().WithEmbeds([vehicleSummaryEmbed]);

        if (vehicleInformation.VehicleMotTests.Count > 0)
            interactionResponse
                .AddEmbeds(motHistoryEmbed)
                .AddComponents(linkButtonComponents);

        await Context.Interaction.SendFollowupMessageAsync(interactionResponse, cancellationToken: cancellationToken);

        tags.Add(new KeyValuePair<string, object?>("registration", vehicleInformation.Registration));
        tags.Add(new KeyValuePair<string, object?>("make", vehicleInformation.Make));
        tags.Add(new KeyValuePair<string, object?>("model", vehicleInformation.Model));
        tags.Add(new KeyValuePair<string, object?>("colour", vehicleInformation.Colour));
        tags.Add(new KeyValuePair<string, object?>("fuel_type", vehicleInformation.FuelType));
        tags.Add(new KeyValuePair<string, object?>("engine_size", vehicleInformation.EngineCapacityLitres));
        foreach (var tag in tags)
            activity?.SetTag(tag.Key, tag.Value);

        Instrumentation.VehicleRegistrationCounter.Add(1, tags);
    }
}