using System.Reflection;
using System.Text.Json.Serialization;
using Amazon.Runtime;
using Amazon.S3;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
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
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        var isOtlpEnabled = !string.IsNullOrWhiteSpace(otlpEndpoint);
        builder.Services.Configure<OpenTelemetryLoggerOptions>(logging =>
        {
            if (isOtlpEnabled)
                logging.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint!));
        });
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            if (isOtlpEnabled)
                logging.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint!);
                    options.Protocol = OtlpExportProtocol.Grpc;
                });
            else
                builder.Logging.AddConsole();
        });

        var otel = builder.Services.AddOpenTelemetry();

        // Configure OpenTelemetry Resources with the application name
        otel.ConfigureResource(resource => resource
            .AddService(serviceName: builder.Environment.ApplicationName));

        // Add Metrics for ASP.NET Core and our custom metrics and export to Prometheus
        otel.WithMetrics(metrics =>
        {
            metrics.AddRuntimeInstrumentation()
                // Metrics provider from OpenTelemetry
                .AddAspNetCoreInstrumentation()
                // Metrics provides by ASP.NET Core in .NET 8
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddMeter("Microsoft.AspNetCore.Http.Connections")
                .AddMeter("Microsoft.AspNetCore.Routing")
                .AddMeter("Microsoft.AspNetCore.Diagnostics")
                .AddMeter("Microsoft.AspNetCore.RateLimiting")
                .AddMeter("Dotbot");

            if (isOtlpEnabled)
                metrics.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint!);
                    options.Protocol = OtlpExportProtocol.Grpc;
                });
            else
                metrics.AddConsoleExporter();

        });

        // Add Tracing for ASP.NET Core and our custom ActivitySource and export to Jaeger
        otel.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation();
            //Add sources here
            tracing.AddSource(builder.Environment.ApplicationName);
            tracing.ConfigureResource(resource =>
                resource.AddService(serviceName: builder.Environment.ApplicationName));
            if (isOtlpEnabled)
                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint!);
                    options.Protocol = OtlpExportProtocol.Grpc;
                });
            else
                tracing.AddConsoleExporter();

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
