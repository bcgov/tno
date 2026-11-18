namespace TNO.Entities;

/// <summary>
/// Provides schedule month control options.
/// </summary>
[Flags]
public enum ScheduleMonth
{
    /// <summary>
    /// Not Applicable
    /// </summary>
    NA = 0,
    /// <summary>
    /// January
    /// </summary>
    January = 1,
    /// <summary>
    /// February
    /// </summary>
    February = 2,
    /// <summary>
    /// March
    /// </summary>
    March = 4,
    /// <summary>
    /// April
    /// </summary>
    April = 8,
    /// <summary>
    /// May
    /// </summary>
    May = 16,
    /// <summary>
    /// June
    /// </summary>
    June = 32,
    /// <summary>
    /// July
    /// </summary>
    July = 64,
    /// <summary>
    /// August
    /// </summary>
    August = 128,
    /// <summary>
    /// September
    /// </summary>
    September = 256,
    /// <summary>
    /// October
    /// </summary>
    October = 512,
    /// <summary>
    /// November
    /// </summary>
    November = 1024,
    /// <summary>
    /// December
    /// </summary>
    December = 2048
}
