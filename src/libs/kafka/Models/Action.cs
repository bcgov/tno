namespace TNO.Kafka.Models;

public class Action
{
    #region Properties
    public string ActionLabel { get; set; } = "";
    public string ActionValue { get; set; } = "";
    #endregion

    #region Constructors
    public Action() { }

    public Action(string actionLabel, string actionValue)
    {
        this.ActionLabel = actionLabel;
        this.ActionValue = actionValue;
    }
    #endregion
}
