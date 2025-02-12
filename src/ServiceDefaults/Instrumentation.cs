using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ServiceDefaults;

public class Instrumentation : IDisposable
{
    internal const string ActivitySourceName = "dotbot";
    internal const string MeterName = "dotbot";
    private readonly Meter _meter;

    public Instrumentation()
    {
        ActivitySource = new ActivitySource(ActivitySourceName);
        _meter = new Meter(MeterName);
        CustomCommandsCounter = _meter.CreateCounter<int>($"{MeterName}.custom.commands.count", description: "The number of custom commands used");
    }

    public ActivitySource ActivitySource { get; }
    public Counter<int> CustomCommandsCounter { get; }

    public void Dispose()
    {
        ActivitySource.Dispose();
        _meter.Dispose();
    }
}