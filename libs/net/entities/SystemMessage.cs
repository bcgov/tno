using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("system_message")]

public class SystemMessage : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Story abstract or message text.
    /// </summary>
    [Column("message")]
    public string Message { get; set; } = "";
    #endregion
    #region Constructors
    protected SystemMessage() { }

    public SystemMessage(string message, string name) : base(name)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(name));
        if (String.IsNullOrWhiteSpace(message)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace", nameof(message));

        this.Message = message;
        this.Name = name;
    }
    #endregion
}
