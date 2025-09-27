using Dotbot.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotbot.Infrastructure.EntityConfigurations;

public class XkcdEntityTypeConfiguration : IEntityTypeConfiguration<Xkcd>
{
    public void Configure(EntityTypeBuilder<Xkcd> xkcdConfiguration)
    {
        xkcdConfiguration.ToTable("xkcd");
    }
}