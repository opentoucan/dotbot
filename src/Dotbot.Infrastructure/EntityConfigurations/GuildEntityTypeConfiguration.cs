using Dotbot.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotbot.Infrastructure.EntityConfigurations;

public class GuildEntityTypeConfiguration : IEntityTypeConfiguration<Guild>
{
    public void Configure(EntityTypeBuilder<Guild> guildConfiguration)
    {
        guildConfiguration.HasMany(g => g.CustomCommands)
            .WithOne();
    }
}