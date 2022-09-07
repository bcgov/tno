
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
    #endregion
}
