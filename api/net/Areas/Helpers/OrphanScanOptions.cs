namespace TNO.API.Helpers;

/// <summary>Conservative, bounded API background reconciliation settings.</summary>
public class OrphanScanOptions
{
    /// <summary>Maximum number of content IDs processed in one batch.</summary>
    public int BatchSize { get; set; } = 200;
    /// <summary>Minimum delay between a scan's batches, in milliseconds.</summary>
    public int BatchDelayMilliseconds { get; set; } = 1000;
    /// <summary>Time allowed for indexing to catch up before confirming a candidate, in seconds.</summary>
    public int GracePeriodSeconds { get; set; } = 300;
    /// <summary>Maximum duration of one batch, in seconds.</summary>
    public int BatchTimeoutSeconds { get; set; } = 30;
}
