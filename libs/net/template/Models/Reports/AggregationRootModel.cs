
namespace TNO.TemplateEngine.Models.Reports;

public class AggregationRootModel {
    #region Properties
    public string Name { get; set; } = "";
    public long DocCount { get; set; } = 0;
    public AggregationModel ChildAggregation { get; set; } = new AggregationModel();
    #endregion
}
