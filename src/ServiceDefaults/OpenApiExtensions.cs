using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace ServiceDefaults;

public static partial class Extensions
{
    public static IApplicationBuilder UseDefaultOpenApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "local")
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        return app;
    }

    public static IHostApplicationBuilder AddDefaultOpenApi(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();

        return builder;
    }
}