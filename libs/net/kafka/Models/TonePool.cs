namespace TNO.Kafka.Models;

public class TonePool
{
    #region Properties
    public long Value { get; set; }
    public string? UserIdentifier { get; set; }

    #endregion

    #region Constructors
    public TonePool() { }

    public TonePool(long toneValue, string userIdentifier)
    {
        this.Value = toneValue;
        this.UserIdentifier = userIdentifier ?? "";
    }
    #endregion
}
