using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ServiceDefaults;

public class Instrumentation : IDisposable
{
    internal const string ActivitySourceName = "dotbot";
    internal const string MeterName = "dotbot";

    public ActivitySource ActivitySource => new(ActivitySourceName);
    private static Meter _meter => new Meter(MeterName);

    public Counter<long> GuildsCounter => _meter.CreateCounter<long>($"{MeterName}.guilds", description: "Counts the number of guilds the bot is in");
    public Counter<long> SavedCustomCommandsCounter => _meter.CreateCounter<long>($"{MeterName}.saved.custom.commands", description: "Counts the number of custom commands saved");
    public Counter<long> CustomCommandsFetchedCounter => _meter.CreateCounter<long>($"{MeterName}.fetched.custom.commands", description: "Counts the number of custom commands fetched");
    public Counter<long> CustomCommandsDeletedCounter => _meter.CreateCounter<long>($"{MeterName}.deleted.custom.commands", description: "Counts the number of custom commands deleted");
    public Counter<long> XkcdCounter => _meter.CreateCounter<long>($"{MeterName}.xkcd", description: "Counts the number of XKCD comics fetched");
    public Counter<long> AvatarCounter => _meter.CreateCounter<long>($"{MeterName}.avatar", description: "Counts the number of avatar requests");
    public Counter<long> VehicleRegistrationCounter => _meter.CreateCounter<long>($"{MeterName}.vehicle.registration", description: "Counts the number of vehicle details fetched by registration");
    public Counter<long> VehicleLinkCounter => _meter.CreateCounter<long>($"{MeterName}.vehicle.link", description: "Counts the number of vehicle details fetched by advert url");
    public Counter<long> ExceptionCounter = _meter.CreateCounter<long>($"{MeterName}.exceptions", description: "Total exceptions thrown");

    public void Dispose()
    {
        ActivitySource.Dispose();
        _meter.Dispose();
        GC.SuppressFinalize(this);
    }
}