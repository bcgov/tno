
using System.Text.RegularExpressions;
using Serilog.Core;
using Serilog.Events;

namespace TNO.Services.Logging;

/// <summary>
/// MessageEnricher record, provides a way to strip new lines from messages for easier logging.
/// </summary>
public class MessageEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.MessageTemplate == null)
            return;

        var escapedMessage = Regex.Replace(logEvent.MessageTemplate.ToString(), @"\r?\n", "\\n");
        var logEventProperty = propertyFactory.CreateProperty("EscapedMessage", escapedMessage);
        logEvent.AddPropertyIfAbsent(logEventProperty);
    }
}
