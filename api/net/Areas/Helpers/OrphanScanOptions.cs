namespace TNO.API.Helpers;

/// <summary>Conservative, bounded API background reconciliation settings.</summary>
public class OrphanScanOptions
{
    public int BatchSize { get; set; } = 200;
    public int BatchDelayMilliseconds { get; set; } = 1000;
    public int GracePeriodSeconds { get; set; } = 300;
    public int BatchTimeoutSeconds { get; set; } = 30;
}
