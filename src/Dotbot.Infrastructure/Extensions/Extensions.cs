using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dotbot.Infrastructure.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddDatabase(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<DbContext>();
        builder.Services.AddDbContext<DotbotContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("dotbot"));
        });
        return builder;
    }
}