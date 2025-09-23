using Dotbot.Infrastructure.Entities.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotbot.Infrastructure.EntityConfigurations;

public class DiscordCommandLogEntityTypeConfiguration : IEntityTypeConfiguration<DiscordCommandLog>
{
    public void Configure(EntityTypeBuilder<DiscordCommandLog> builder)
    {
        builder.HasKey(mc => mc.Id);
    }
}