using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.System.Text.Json;

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
        builder.Services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(new RedisConfiguration
        {
            ConnectionString = builder.Configuration.GetConnectionString("redis")
        });
        if (builder.Environment.EnvironmentName == "local")
        {
            using var scope = builder.Services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DotbotContext>();
            context.Database.EnsureDeleted();
            context.Database.Migrate();
            context.SaveChanges();
        }

        return builder;
    }
}