using System.Text.Json.Serialization;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ServiceDefaults;

public static partial class Extensions
{

    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.AddBasicServiceDefaults();
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

        });
        builder.AddDefaultOpenApi();
        return builder;
    }

    /// <summary>
    /// Adds the services except for making outgoing HTTP calls.
    /// </summary>
    /// <remarks>
    /// This allows for things like Polly to be trimmed out of the app if it isn't used.
    /// </remarks>
    public static IHostApplicationBuilder AddBasicServiceDefaults(this IHostApplicationBuilder builder)
    {
        // Default health checks assume the event bus and self health checks
        builder.AddDefaultHealthChecks();

        builder.ConfigureOpenTelemetry();

        return builder;
    }


    private static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<Instrumentation>();
        var otlpEndpoint = builder.Configuration["OTLP_ENDPOINT_URL"];
        var otel = builder.Services.AddOpenTelemetry();

        // Configure OpenTelemetry Resources with the application name
        otel.ConfigureResource(resource => resource
            .AddService(serviceName: builder.Environment.ApplicationName));

        // Add Metrics for ASP.NET Core and our custom metrics and export to Prometheus
        otel.WithMetrics(metrics =>
        {
            metrics.AddRuntimeInstrumentation()
                // Metrics provides by ASP.NET Core
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                // Metrics provided by System.Net libraries
                .AddMeter("System.Net.Http")
                .AddMeter("System.Net.NameResolution")
                .AddMeter(Instrumentation.MeterName)
                .SetExemplarFilter(ExemplarFilterType.TraceBased)
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddPrometheusExporter();
        });

        // Add Tracing for ASP.NET Core and our custom ActivitySource and export to Jaeger
        otel.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation()
                .AddSource(Instrumentation.ActivitySourceName)
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation();
            if (otlpEndpoint != null)
            {
                tracing.AddOtlpExporter(options => { options.Endpoint = new Uri(otlpEndpoint); });
            }
            else
            {
                tracing.AddConsoleExporter();
            }
        });

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapHealthChecks("/health")
            .AllowAnonymous();

        app.MapPrometheusScrapingEndpoint();
        return app;
    }

    public static IHostApplicationBuilder ConfigureAWS(this IHostApplicationBuilder builder)
    {
        var awsOptions = builder.Configuration.GetAWSOptions<AmazonS3Config>("S3");
        awsOptions.DefaultClientConfig.RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED;
        awsOptions.DefaultClientConfig.ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED;

        builder.Services.AddDefaultAWSOptions(awsOptions);
        builder.Services.AddAWSService<IAmazonS3>();
        return builder;
    }

}