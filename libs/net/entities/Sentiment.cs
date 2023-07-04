using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Sentiment class, provides a DB model to manage sentiment settings.
/// </summary>
[Cache("sentiment")]
[Table("sentiment")]
public class Sentiment : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - The value of the sentiment (normally -5 to 5).
    /// </summary>
    [Column("value")]
    public float Value { get; set; }

    /// <summary>
    /// get/set - The percentage to apply to each value in the earned media formula.
    /// </summary>
    [Column("rate")]
    public float Rate { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Sentiment object.
    /// </summary>
    protected Sentiment() : base() { }

    /// <summary>
    /// Creates a new instance of a Sentiment object, initializes with specified parameters.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="contentType"></param>
    /// <param name="lengthOfContent"></param>
    /// <param name="rate"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Sentiment(string name, float value, float rate) : base(name)
    {
        this.Value = value;
        this.Rate = rate;
    }
    #endregion
}
