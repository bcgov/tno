
using System.Text.RegularExpressions;
using Serilog.Core;
using Serilog.Events;

namespace TNO.Services.Logging;

/// <summary>
/// ExceptionEnricher record, provides a way to strip new lines from exceptions for easier logging.
/// </summary>
public class ExceptionEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Exception == null)
            return;

        var escapedException = Regex.Replace(logEvent.Exception.ToString(), @"\r?\n", "\\n");
        var logEventProperty = propertyFactory.CreateProperty("EscapedException", escapedException);
        logEvent.AddPropertyIfAbsent(logEventProperty);
    }
}
