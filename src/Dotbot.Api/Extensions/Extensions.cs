using Dotbot.Api.Application.Queries;
using Dotbot.Api.Queries;
using Dotbot.Api.Services;
using Dotbot.Api.Settings;
using Dotbot.Infrastructure.Extensions;
using Dotbot.Infrastructure.Repositories;
using ServiceDefaults;

namespace Dotbot.Api.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.ConfigureXkcd();
        builder.ConfigureVehicleService();
        builder.ConfigureMoturService();
        builder.Services.AddScoped<IFileUploadService, FileUploadService>();
        builder.Services.AddScoped<IGuildRepository, GuildRepository>();
        builder.Services.AddScoped<IGuildQueries, GuildQueries>();
        builder.Services.AddScoped<ICustomCommandService, CustomCommandService>();
        builder.AddDatabase();
        builder.ConfigureAWS();
        return builder;
    }

    public static IHostApplicationBuilder ConfigureXkcd(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<IXkcdService, XkcdService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("XkcdUrl")!);
            })
            .AddStandardResilienceHandler();

        return builder;
    }

    public static IHostApplicationBuilder ConfigureVehicleService(this IHostApplicationBuilder builder)
    {
        var vehicleEnquirySection = builder.Configuration.GetSection("VehicleEnquiry");
        var vehicleEnquirySettings = vehicleEnquirySection.Get<VehicleEnquirySettings>();
        builder.Services.AddOptions<VehicleEnquirySettings>().Bind(vehicleEnquirySection);

        var motHistorySection = builder.Configuration.GetSection("MotHistory");
        var motHistorySettings = motHistorySection.Get<MotHistorySettings>();
        builder.Services.AddOptions<MotHistorySettings>().Bind(motHistorySection);

        builder.Services.AddHttpClient<IVehicleEnquiryService, VehicleEnquiryEnquiryService>(client =>
            {
                client.BaseAddress = new Uri(vehicleEnquirySettings!.Url);
                client.DefaultRequestHeaders.Add("x-api-key", vehicleEnquirySettings.ApiKey);
            })
            .AddStandardResilienceHandler();

        builder.Services.AddHttpClient<IMotHistoryService, MotHistoryService>(client =>
            {
                client.BaseAddress = new Uri(motHistorySettings!.Url);
                client.DefaultRequestHeaders.Add("X-Api-Key", motHistorySettings.ApiKey);
            })
            .AddStandardResilienceHandler();

        return builder;
    }

    public static IHostApplicationBuilder ConfigureMoturService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<IMoturService, MoturService>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("MoturUrl")!);
            })
            .AddStandardResilienceHandler();

        return builder;
    }
}