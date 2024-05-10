DO $$
BEGIN

INSERT INTO public."chart_template" (
  "name"
  , "description"
  , "is_enabled"
  , "is_public"
  , "sort_order"
  , "template"
  , "settings"
  , "created_by"
  , "updated_by"
) VALUES (
  'Custom' -- name
  , 'A custom chart that can be configured to support most dataset.' -- description
  , true -- is_enabled
  , true -- is_public
  , 0 -- sort_order
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using TNO.Entities
@using TNO.TemplateEngine

@{
  var datasets = ReportExtensions.GroupContent(SectionSettings.Dataset, Content, Sections);
  var groups = ReportExtensions.GroupContent(SectionSettings.GroupBy, Content, Sections);
  var labels = ReportExtensions.GetLabels(SectionSettings.GroupBy, groups, Sections);
  var datasetIndex = 0;
}
{
  "labels": [@String.Join(",", labels.Select(v => $"\"{v}\""))],
  "datasets": [@String.Join(", ", datasets.Select((ds) => $"{{ " +
    $"\"label\": \"{ds.Key}\", " +
    $"\"data\": [{String.Join(", ", ReportExtensions.ExtractDatasetValues(SectionSettings.DatasetValue, SectionSettings.GroupBy, ds, labels).Select(v => v.HasValue ? v.ToString() : "null"))}], " +
    $"\"backgroundColor\": \"{ReportExtensions.GetColor(SectionSettings.DatasetColors, datasetIndex, SectionSettings.Dataset, ds.Key, SectionSettings.DatasetValue)}\", " +
    $"\"spanGaps\": true, " +
    $"\"borderWidth\": 2, " +
    $"\"minBarLength\": {SectionSettings.MinBarLength?.ToString() ?? "null"}, " +
    $"\"borderColor\": \"{ReportExtensions.GetColor(SectionSettings.DatasetColors, datasetIndex++, SectionSettings.Dataset, ds.Key, SectionSettings.DatasetValue)}\" }}"))]
}' -- template
  , '{"dataset": ["", "mediaType", "source", "series", "byline", "contentType", "topicType", "topicName", "sentiment", "sentimentSimple", "dayMonthYear", "monthYear", "year", "reportSection"], "groupBy": ["mediaType", "source", "series", "byline", "contentType", "topicType", "topicName", "sentiment", "sentimentSimple", "dayMonthYear", "monthYear", "year", "reportSection"], "options": {"scales": {"x": {"ticks": {}, "title": {"display": false}}, "y": {"ticks": {}, "title": {"display": false}}}, "plugins": {"title": {"display": false}, "colors": {}, "legend": {"title": {"display": false}, "display": true}, "subtitle": {"display": false}, "datalabels": {"anchor": "center", "labels": {"title": {}}}}, "indexAxis": "x"}, "chartTypes": ["bar", "line", "pie", "doughnut", "scatter", "bubble", "polarArea", "radar"], "datasetValue": ["count", "sentiment"]}' -- settings
  , '' -- created_by
  , '' -- updated_by
) ON CONFLICT DO NOTHING;

END $$;
