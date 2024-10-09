using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotbot.Infrastructure.EntityConfigurations;

public class XkcdEntityTypeConfiguration : IEntityTypeConfiguration<Entities.Xkcd>
{
    public void Configure(EntityTypeBuilder<Entities.Xkcd> xkcdConfiguration)
    {
        xkcdConfiguration.ToTable("xkcds");
    }
}