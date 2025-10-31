using Dotbot.Infrastructure.Entities.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.System.Text.Json;

namespace Dotbot.Infrastructure.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddDatabase(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<DotbotContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("dotbot"),
                    o => o.AddPostgresOptions())
                .AddDbContextOptions();
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

    public static DbContextOptionsBuilder AddDbContextOptions(
        this DbContextOptionsBuilder builder)
    {
        builder.UseSnakeCaseNamingConvention();
        return builder;
    }

    public static NpgsqlDbContextOptionsBuilder AddPostgresOptions(this NpgsqlDbContextOptionsBuilder builder)
    {
        builder
            .MapEnum<MotDefectCategory>(schemaName: "dotbot")
            .MapEnum<TestResult>(schemaName: "dotbot")
            .MapEnum<OdometerResult>(schemaName: "dotbot")
            .MapEnum<FuelType>(schemaName: "dotbot");
        return builder;
    }
}