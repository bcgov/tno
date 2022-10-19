
using TNO.Services.Config;

namespace TNO.Services.NLP.Config;

/// <summary>
/// NLPOptions class, configuration options for NLP
/// </summary>
public class NLPOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - A comma separated list of Kafka topics to consume.
    /// </summary>
    public string Topics { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka topic to send indexing requests to.
    /// </summary>
    public string IndexingTopic { get; set; } = "";

    /// <summary>
    /// get/set - The number of attempts to retry a failed import.
    /// A retry that ultimately fails will still only count as a single failure for the service.
    /// </summary>
    public int RetryLimit { get; set; } = 3;

    /// <summary>
    /// get/set - Whether to only accept messages from Kafka that include work orders.
    /// </summary>
    public bool AcceptOnlyWorkOrders { get; set; } = true;
    #endregion
}
