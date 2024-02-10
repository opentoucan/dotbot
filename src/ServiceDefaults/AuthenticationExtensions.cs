using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ServiceDefaults;

public static class AuthenticationExtensions
{
    public static IHostApplicationBuilder AddDefaultAuthentication(this IHostApplicationBuilder builder)
    {
        var identitySection = builder.Configuration.GetSection("Identity");

        if (!identitySection.Exists())
        {
            // No identity section, so no authentication
            return builder;
        }

        return builder;
    }
}