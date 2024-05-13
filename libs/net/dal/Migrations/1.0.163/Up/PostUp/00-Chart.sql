DO $$
BEGIN

UPDATE public."chart_template"
SET "template" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using TNO.Entities
@using TNO.TemplateEngine

@{
  var datasets = ReportExtensions.GroupContent(SectionSettings.Dataset, Content, Sections);
  var groups = ReportExtensions.GroupContent(SectionSettings.GroupBy, Content, Sections);
  var labels = ReportExtensions.GetLabels(groups, SectionSettings, Sections);
  var datasetIndex = 0;
}
{
  "labels": [@String.Join(",", labels.Select(v => $"\"{v}\""))],
  "datasets": [@String.Join(", ", datasets.Select((ds) => $"{{ " +
    $"\"label\": \"{ReportExtensions.GetLabel(ds, SectionSettings, Sections)}\", " +
    $"\"data\": [{String.Join(", ", ReportExtensions.ExtractDatasetValues(SectionSettings.DatasetValue, SectionSettings.GroupBy, ds, labels).Select(v => v.HasValue ? v.ToString() : "null"))}], " +
    $"\"backgroundColor\": \"{ReportExtensions.GetColor(SectionSettings.DatasetColors, datasetIndex, SectionSettings.Dataset, ds.Key, SectionSettings.DatasetValue)}\", " +
    $"\"spanGaps\": true, " +
    $"\"borderWidth\": 2, " +
    $"\"minBarLength\": {SectionSettings.MinBarLength?.ToString() ?? "null"}, " +
    $"\"borderColor\": \"{ReportExtensions.GetColor(SectionSettings.DatasetColors, datasetIndex++, SectionSettings.Dataset, ds.Key, SectionSettings.DatasetValue)}\" }}"))]
}'
  , "settings" = '{"dataset": ["", "mediaType", "source", "series", "byline", "contentType", "topicType", "topicName", "sentiment", "sentimentSimple", "dayMonthYear", "monthYear", "year", "reportSection"], "groupBy": ["mediaType", "source", "series", "byline", "contentType", "topicType", "topicName", "sentiment", "sentimentSimple", "dayMonthYear", "monthYear", "year", "reportSection", ""], "options": {"scales": {"x": {"ticks": {}, "title": {"display": false}}, "y": {"ticks": {}, "title": {"display": false}}}, "plugins": {"title": {"display": false}, "colors": {}, "legend": {"title": {"display": false}, "display": true}, "subtitle": {"display": false}, "datalabels": {"anchor": "center", "labels": {"title": {}}}}, "indexAxis": "x"}, "chartTypes": ["bar", "line", "pie", "doughnut", "scatter", "bubble", "polarArea", "radar"], "datasetValue": ["count", "sentiment"]}'
WHERE "name" = 'Custom';

END $$;
