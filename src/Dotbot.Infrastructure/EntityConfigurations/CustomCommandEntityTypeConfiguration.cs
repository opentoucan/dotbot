using Dotbot.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotbot.Infrastructure.EntityConfigurations;

public class CustomCommandEntityTypeConfiguration : IEntityTypeConfiguration<CustomCommand>
{
    public void Configure(EntityTypeBuilder<CustomCommand> customCommandConfiguration)
    {
        customCommandConfiguration.HasMany(cc => cc.Attachments)
            .WithOne();
    }
}