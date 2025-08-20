using System.Diagnostics;
using System.Text.Json;
using Ardalis.Result;
using Dotbot.Api.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using ServiceDefaults;

namespace Dotbot.Api.Application;

[SlashCommand("mot", "Retrieves MOT history")]
public class MotHistoryCommandModule(IMotService motService, Instrumentation instrumentation, ILogger<MotHistoryCommandModule> logger) : ApplicationCommandModule<HttpApplicationCommandContext>
{
    [SubSlashCommand("plate", "Registration plate to search")]
    public async Task RetreiveByPlate(
        [SlashCommandParameter(Name = "plate", Description = "Registration plate")]
        string reg)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(ActivityKind.Client);
        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
        var result = await motService.GetMotByRegistrationPlate(reg);
        await RetrieveMotHistoryAsync(activity, result);
    }

    [SubSlashCommand("link", "URL of a vehicle advert i.e. autotrader or carandclassic ad")]
    public async Task RetrieveByLink(
        [SlashCommandParameter(Name = "link", Description = "URL")]
        string link)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(ActivityKind.Client);
        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
        var result = await motService.GetMotByVehicleAdvert(link);
        await RetrieveMotHistoryAsync(activity, result);
    }


    private async Task RetrieveMotHistoryAsync(Activity? activity, Result<MotHistory> result)
    {

        if (!result.IsSuccess)
        {
            await Context.Interaction.SendFollowupMessageAsync("An error occurred while retrieving MOT history.");
            instrumentation.ExceptionCounter.Add(1,
            new KeyValuePair<string, object?>("type", result.Errors.FirstOrDefault()),
            new KeyValuePair<string, object?>("interaction_name", "mot")
            );
        }
        else
        {
            var embed = new EmbedProperties();
            var green = new Color(0, 128, 0);
            var orange = new Color(255, 140, 0);
            var red = new Color(255, 0, 0);
            embed.WithTitle("Vehicle Information");
            var hasRecordedMotTestDate = DateTime.TryParse(result.Value.motTests?.OrderByDescending(x => x.completedDate).FirstOrDefault()?.expiryDate, out var lastMotExpiryDate);
            var hasValidMot = hasRecordedMotTestDate && lastMotExpiryDate > DateTimeOffset.UtcNow;
            var motDescription = "";
            Color vehicleEmbedColor;
            var registrationDateTime = DateTime.Parse(result.Value.registrationDate);
            var firstMotRequiredDate = registrationDateTime.AddYears(3);
            if (firstMotRequiredDate > DateTimeOffset.UtcNow)
            {
                vehicleEmbedColor = green;
                motDescription = $"No MOT required until {firstMotRequiredDate.ToShortDateString()} (in {(int)Math.Round((firstMotRequiredDate - DateTimeOffset.UtcNow).TotalDays)} days)";
            }
            else if (hasValidMot)
            {
                vehicleEmbedColor = green;
                motDescription = $"MOT valid until {lastMotExpiryDate.ToShortDateString()} (for {(int)Math.Round((lastMotExpiryDate - DateTimeOffset.UtcNow).TotalDays)} days)";
            }
            else
            {
                vehicleEmbedColor = red;
                var lastRecordedMotOrRegistrationDate = hasRecordedMotTestDate ? lastMotExpiryDate : registrationDateTime.AddYears(3);
                motDescription = $"MOT expired on {lastRecordedMotOrRegistrationDate.ToShortDateString()} ({(int)Math.Round((DateTimeOffset.UtcNow - lastRecordedMotOrRegistrationDate).TotalDays)} days ago)";
            }
            var motTestsOrderedByDate = result.Value.motTests?.OrderByDescending(test => test.completedDate);
            var latestMot = motTestsOrderedByDate?.FirstOrDefault();
            embed.WithDescription(motDescription);
            embed.WithColor(vehicleEmbedColor);

            var hasEngineSize = decimal.TryParse(result.Value.engineSize, out var engineSizeDecimal);
            embed.AddFields(new EmbedFieldProperties().WithName("Registration plate").WithValue(result.Value.registration.ToUpper()).WithInline());
            embed.AddFields(new EmbedFieldProperties().WithName("Make").WithValue(result.Value.make).WithInline());
            embed.AddFields(new EmbedFieldProperties().WithName("Model").WithValue(result.Value.model).WithInline());
            embed.AddFields(new EmbedFieldProperties().WithName("Colour").WithValue(result.Value.primaryColour).WithInline());
            embed.AddFields(new EmbedFieldProperties().WithName("Fuel Type").WithValue(result.Value.fuelType).WithInline());
            embed.AddFields(new EmbedFieldProperties().WithName("Engine Size").WithValue(hasEngineSize ? $"{decimal.Round(engineSizeDecimal / 1000, 1)}L" : "Unknown").WithInline());
            embed.AddFields(new EmbedFieldProperties().WithName("Registration date").WithValue(result.Value.registrationDate).WithInline());
            embed.AddFields(new EmbedFieldProperties().WithName("Odometer").WithValue(latestMot is not null ? $"{latestMot?.odometerValue} {latestMot?.odometerUnit}" : "Unknown").WithInline());

            var motEmbeds = new List<EmbedProperties> { embed };
            foreach (var test in motTestsOrderedByDate?.Take(5) ?? [])
            {
                var motEmbed = new EmbedProperties();
                var motEmbedColor = test.testResult == "PASSED" && test.defects.Count == 0 ? green :
                                        test.testResult == "PASSED" ? orange :
                                         red;

                motEmbed.WithColor(motEmbedColor);
                motEmbed.WithTitle($"MOT {test.completedDate.ToShortDateString()} - {test.testResult}");
                if (test.testResult == "PASSED")
                    motEmbed.AddFields(new EmbedFieldProperties().WithName("Expires").WithValue(DateTime.Parse(test.expiryDate!).ToLongDateString()).WithInline());
                motEmbed.AddFields(new EmbedFieldProperties().WithName("Odometer reading").WithValue($"{test.odometerValue} {test.odometerUnit}").WithInline());

                var groupedDefects = test.defects
                    .OrderBy(defect => defect.type)
                    .GroupBy(defect => defect.type);
                foreach (var defectGroup in groupedDefects)
                {
                    var embedFieldLengthLimit = 1024;
                    var defectText = string.Join("\n", defectGroup.Select(x => $"- {x.text}"));

                    var defectChunks = defectGroup.Chunk(defectGroup.Count() / defectText.Chunk(embedFieldLengthLimit).Count());

                    foreach (var chunk in defectChunks)
                    {
                        motEmbed.AddFields(new EmbedFieldProperties().WithName(chunk.FirstOrDefault()?.type.ToString()).WithValue(string.Join("\n", chunk.Select(x => $"- {x.text}"))));
                    }
                }

                motEmbeds.Add(motEmbed);
            }

            var interactionResponse = new InteractionMessageProperties().WithEmbeds(motEmbeds);
            logger.LogInformation($"Message length: {JsonSerializer.Serialize(interactionResponse).Length}");
            await Context.Interaction.SendFollowupMessageAsync(interactionResponse);
            var displayName = (Context.Interaction.User as GuildUser)?.Nickname ?? Context.Interaction.User.GlobalName ?? Context.Interaction.User.Username;

            var tags = new TagList
                {
                    { "command_name", "mot" },
                    { "member_id", Context.Interaction.User.Id },
                    { "member_display_name", displayName }
                };

            foreach (var tag in tags)
                activity?.SetTag(tag.Key, tag.Value);

            instrumentation.SavedCustomCommandsCounter.Add(1, tags);
        }

    }
}