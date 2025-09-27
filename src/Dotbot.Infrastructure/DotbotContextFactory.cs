using Dotbot.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dotbot.Infrastructure;

public class DotbotContextFactory : IDesignTimeDbContextFactory<DotbotContext>
{
    public DotbotContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DotbotContext>();
        optionsBuilder.UseNpgsql(o => o.AddPostgresOptions())
            .AddDbContextOptions();
        return new DotbotContext(optionsBuilder.Options);
    }
}