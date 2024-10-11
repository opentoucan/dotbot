using System.Security.Claims;
using Dotbot.Gateway.Apis.Auth;

namespace Dotbot.Gateway.Extensions;

public static partial class Extensions
{
    public static IHostApplicationBuilder AddDiscordInteractionAuth(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthentication()
            .AddScheme<DiscordSignatureAuthenticationSchemeOptions, DiscordSignatureAuthenticationHandler>("DiscordSignature",
                options =>
                {
                    builder.Configuration.GetSection(nameof(Settings.Discord)).Bind(options);
                });
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("DiscordSignature", policyBuilder =>
            {
                policyBuilder.AddAuthenticationSchemes("DiscordSignature");
                policyBuilder.RequireClaim(ClaimTypes.Name, ["service"]);
            });
        });
        
        return builder;
    }
}