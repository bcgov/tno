using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using TNO.API.Config;
using TNO.DAL;
using TNO.Kafka;
using TNO.Kafka.Models;

namespace TNO.API.Filters;

/// <summary>
/// AnalysisWakeFilter class, after a request that scheduled analysis work, sends a message on the
/// Kafka analysis topic so idle Content-Analysis workers wake when the work becomes due. When Kafka
/// does not accept it the request fails, since the system is down.
/// </summary>
public class AnalysisWakeFilter : IAsyncActionFilter
{
    #region Variables
    private readonly TNOContext _context;
    private readonly IKafkaMessenger _kafka;
    private readonly KafkaOptions _options;
    private readonly ILogger<AnalysisWakeFilter> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AnalysisWakeFilter object, initializes with specified parameters.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="kafka"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public AnalysisWakeFilter(TNOContext context, IKafkaMessenger kafka, IOptions<KafkaOptions> options, ILogger<AnalysisWakeFilter> logger)
    {
        _context = context;
        _kafka = kafka;
        _options = options.Value;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Run the action, then wake the workers when it scheduled analysis.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();
        try
        {
            if (_context.ScheduledAnalysisJobs.Count == 0 || (executed.Exception != null && !executed.ExceptionHandled)) return;
            if (String.IsNullOrWhiteSpace(_options.AnalysisTopic))
            {
                _logger.LogWarning("Kafka analysis topic not configured.");
                return;
            }
            var dueOn = _context.ScheduledAnalysisJobs.Min();
            await _kafka.SendMessageAsync(_options.AnalysisTopic, "analysis", new AnalysisRequestModel(dueOn, _context.ScheduledAnalysisJobs.Count));
        }
        finally
        {
            _context.ScheduledAnalysisJobs.Clear();
        }
    }
    #endregion
}
