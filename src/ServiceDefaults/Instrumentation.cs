using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ServiceDefaults;

public class Instrumentation : IDisposable
{
    internal const string ActivitySourceName = "dotbot";
    internal const string MeterName = "dotbot";

    public static Counter<long> ExceptionCounter =
        Meter.CreateCounter<long>($"{MeterName}.exceptions", description: "Total exceptions thrown");

    public static ActivitySource ActivitySource => new(ActivitySourceName);
    private static Meter Meter => new(MeterName);

    public static Counter<long> GuildsCounter => Meter.CreateCounter<long>($"{MeterName}.guilds",
        description: "Counts the number of guilds the bot is in");

    public static Counter<long> SavedCustomCommandsCounter => Meter.CreateCounter<long>(
        $"{MeterName}.saved.custom.commands",
        description: "Counts the number of custom commands saved");

    public static Counter<long> CustomCommandsFetchedCounter => Meter.CreateCounter<long>(
        $"{MeterName}.fetched.custom.commands", description: "Counts the number of custom commands fetched");

    public static Counter<long> CustomCommandsDeletedCounter => Meter.CreateCounter<long>(
        $"{MeterName}.deleted.custom.commands", description: "Counts the number of custom commands deleted");

    public static Counter<long> XkcdCounter =>
        Meter.CreateCounter<long>($"{MeterName}.xkcd", description: "Counts the number of XKCD comics fetched");

    public static Counter<long> AvatarCounter =>
        Meter.CreateCounter<long>($"{MeterName}.avatar", description: "Counts the number of avatar requests");

    public static Counter<long> VehicleRegistrationCounter => Meter.CreateCounter<long>(
        $"{MeterName}.vehicle.registration",
        description: "Counts the number of vehicle details fetched by registration");

    public static Counter<long> VesApiErrorCounter => Meter.CreateCounter<long>($"{MeterName}.ves.api.errors",
        description: "Counts the errors returned by the ves API");

    public static Counter<long> MotApiErrorCounter => Meter.CreateCounter<long>($"{MeterName}.mot.api.errors",
        description: "Counts the errors returned by the mot API");

    public void Dispose()
    {
        ActivitySource.Dispose();
        Meter.Dispose();
        GC.SuppressFinalize(this);
    }
}