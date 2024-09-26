using Bot.Gateway.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bot.Gateway.Infrastructure.EntityConfigurations;

public class CustomCommandEntityTypeConfiguration : IEntityTypeConfiguration<CustomCommand>
{
    public void Configure(EntityTypeBuilder<CustomCommand> customCommandConfiguration)
    {
        customCommandConfiguration.ToTable("commands");

        customCommandConfiguration.Ignore(b => b.DomainEvents);
        
        customCommandConfiguration.HasMany(cc => cc.Attachments)
            .WithOne();
        customCommandConfiguration
            .OwnsOne(o => o.Guild);
    }
}