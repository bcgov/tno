namespace TNO.Models.Kafka;

public class Tag
{
    #region Properties
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    public Tag() { }

    public Tag(string key, string value)
    {
        this.Key = key ?? "";
        this.Value = value ?? "";
    }
    #endregion
}
