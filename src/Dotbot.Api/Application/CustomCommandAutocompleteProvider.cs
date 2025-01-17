using Dotbot.Api.Application.Queries;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Dotbot.Api.Application;

public class CustomCommandAutocompleteProvider(IGuildQueries guildQueries) : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public async ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        var guildId = context.Interaction.GuildId;
        if (guildId == null) return Array.Empty<ApplicationCommandOptionChoiceProperties>();
        var customCommands = await guildQueries.GetCustomCommandsByFuzzySearchOnNameAsync(guildId.GetValueOrDefault().ToString(), option.Value ?? string.Empty);
        return customCommands.Select(customCommand => new ApplicationCommandOptionChoiceProperties(customCommand.Name, customCommand.Name));
    }
}