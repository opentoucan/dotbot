using System.Diagnostics;
using Ardalis.Result;
using Dotbot.Api.Helpers;
using Dotbot.Api.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using ServiceDefaults;

namespace Dotbot.Api.Application;

[SlashCommand("mot", "Retrieves MOT history")]
public class MotHistoryCommandModule(IMotService motService, Instrumentation instrumentation) : ApplicationCommandModule<HttpApplicationCommandContext>
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

    private async Task RetrieveMotHistoryAsync(Activity? activity, Result<MoturResponse> result)
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
            var moturResponse = result.Value;
            var embeds = new List<EmbedProperties>();
            var vehicleSummaryEmbed = new EmbedProperties();
            if (result.Value.MotResponse?.ErrorDetails?.HttpStatusCode != null && result.Value.RegistrationResponse?.ErrorDetails != null)
            {
                vehicleSummaryEmbed.WithTitle("Could not retrieve information about this vehicle");
                if (result.Value.MotResponse?.ErrorDetails?.HttpStatusCode == 404 && (result.Value.RegistrationResponse?.ErrorDetails?.HttpStatusCode == 404 || result.Value.RegistrationResponse?.ErrorDetails?.HttpStatusCode == 400))
                {
                    vehicleSummaryEmbed.AddFields(new EmbedFieldProperties().WithName("DVLA").WithValue(moturResponse.RegistrationResponse?.ErrorDetails?.Reason));
                    vehicleSummaryEmbed.AddFields(new EmbedFieldProperties().WithName("DVSA").WithValue(moturResponse.MotResponse?.ErrorDetails?.Reason));
                }
                else
                {
                    vehicleSummaryEmbed.AddFields(new EmbedFieldProperties().WithName("This error was unexpected, try again later"));
                }
            }
            else
            {
                vehicleSummaryEmbed = DiscordEmbedHelper.BuildVehicleInformationEmbed(moturResponse);
            }

            embeds.Add(vehicleSummaryEmbed);

            var reg = moturResponse?.RegistrationResponse?.Details?.RegistrationPlate ?? moturResponse?.MotResponse?.Details?.RegistrationPlate;
            var motTests = moturResponse?.MotResponse?.Details?.MotTests ?? [];

            embeds.Add(DiscordEmbedHelper.BuildMotSummaryEmbed(motTests));

            var interactionResponse = new InteractionMessageProperties()
                .WithEmbeds(embeds)
                .WithComponents([new ActionRowProperties().AddComponents(new LinkButtonProperties($"https://www.check-mot.service.gov.uk/results?registration={reg}", label: "Link to full MOT History"))]);
            await Context.Interaction.SendFollowupMessageAsync(interactionResponse.WithEmbeds(embeds));

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