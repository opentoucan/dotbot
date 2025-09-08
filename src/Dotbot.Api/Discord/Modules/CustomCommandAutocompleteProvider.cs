using Dotbot.Api.Queries;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Dotbot.Api.Discord.Modules;

public class CustomCommandAutocompleteProvider(IGuildQueries guildQueries)
    : IAutocompleteProvider<HttpAutocompleteInteractionContext>
{
    public async ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(
        ApplicationCommandInteractionDataOption option, HttpAutocompleteInteractionContext context)
    {
        var guildId = context.Interaction.GuildId;
        if (guildId == null) return [];
        var customCommands =
            await guildQueries.GetCustomCommandsByFuzzySearchOnNameAsync(guildId.GetValueOrDefault().ToString(),
                option.Value ?? string.Empty);
        return customCommands.Select(customCommand =>
            new ApplicationCommandOptionChoiceProperties(customCommand.Name, customCommand.Name));
    }
}