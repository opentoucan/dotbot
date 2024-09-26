using Bot.Gateway.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bot.Gateway.FunctionalTests.Setup;

public static class ServiceCollectionExtensions
{
    public static void RemoveDbContext<T>(this IServiceCollection services) where T : DbContext
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<T>));
        if (descriptor != null) services.Remove(descriptor);
    }

    public static void EnsureDbCreated<T>(this IServiceCollection services) where T : DbContext
    {
        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var context = scopedServices.GetRequiredService<T>();
        context.Database.EnsureCreated();
    }

    public static void AddMockAuthentication(this IServiceCollection services)
    {
        services.RemoveAll<DiscordSignatureAuthenticationHandler>();
        services.RemoveAll<AuthenticationHandler<DiscordSignatureAuthenticationSchemeOptions>>();
        services.RemoveAll<DiscordSignatureAuthenticationSchemeOptions>();
        services.AddTransient<IAuthenticationSchemeProvider, MockSchemeProvider>();
        services.Configure<AuthenticationOptions>(o =>
        {
            o.SchemeMap.Clear();
            ((IList<AuthenticationSchemeBuilder>) o.Schemes).Clear();
        });
        services
            .AddAuthentication("DiscordSignature")
            .AddScheme<AuthenticationSchemeOptions, MockAuthenticationHandler>("DiscordSignature", null);
    }
}