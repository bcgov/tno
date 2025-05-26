using RazorEngineCore;

namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// ReportEngineDataModel class, provides a model to pass to the razor engine.
/// </summary>
public class ReportEngineDataModel<T> : RazorEngineTemplateBase
{
    #region Properties
    /// <summary>
    /// get/set - The main model.
    /// </summary>
    public T Data { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TemplateModel, initializes with specified parameters.
    /// </summary>
    public ReportEngineDataModel()
    {
        this.Data = default!;
    }

    /// <summary>
    /// Creates a new instance of a TemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="data"></param>
    public ReportEngineDataModel(T data)
    {
        this.Data = data;
    }
    #endregion
}
