using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TNO.Elastic.Models.Converters;

namespace TNO.Elastic.Models;

public class AggregationRootModel {
    #region Properties
    public string Name { get; set; }
    public long DocCount { get; set; }
    public AggregationModel ChildAggregation { get; set; }
    #endregion
}
