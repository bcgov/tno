// CustomLevelEnricher.cs
using Serilog.Core;
using Serilog.Events;

namespace TNO.Services.Logging;
public class CustomLevelEnricher : ILogEventEnricher
{
    private const string PropertyName = "CustomLevel";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent == null) return;

        // If a caller already set CustomLevel (via ForContext), don't override it
        if (logEvent.Properties.ContainsKey(PropertyName)) return;

        var mapped = logEvent.Level switch
        {
            LogEventLevel.Debug => "debug",
            LogEventLevel.Information => "info",
            LogEventLevel.Warning => "warning",
            LogEventLevel.Error => "error",
            LogEventLevel.Fatal => "critical",
            LogEventLevel.Verbose => "debug",
            _ => logEvent.Level.ToString().ToLowerInvariant()
        };

        var prop = propertyFactory.CreateProperty(PropertyName, mapped);
        logEvent.AddPropertyIfAbsent(prop);
    }
}
