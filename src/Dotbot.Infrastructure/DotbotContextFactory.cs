using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dotbot.Infrastructure;

public class DotbotContextFactory : IDesignTimeDbContextFactory<DotbotContext>
{
    public DotbotContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DotbotContext>();
        optionsBuilder.UseNpgsql();
        return new DotbotContext(optionsBuilder.Options);
    }
}