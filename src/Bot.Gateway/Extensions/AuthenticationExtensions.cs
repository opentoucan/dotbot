using System.Security.Claims;
using Bot.Gateway.Apis.Auth;

namespace Bot.Gateway.Extensions;

public static partial class Extensions
{
    public static IHostApplicationBuilder AddDiscordInteractionAuth(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthentication()
            .AddScheme<DiscordSignatureAuthenticationSchemeOptions, DiscordSignatureAuthenticationHandler>("DiscordSignature",
                options =>
                {
                    builder.Configuration.GetSection("DiscordSettings").Bind(options);
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