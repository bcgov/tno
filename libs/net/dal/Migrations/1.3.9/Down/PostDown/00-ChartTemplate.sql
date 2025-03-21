DO $$
BEGIN

-- Update custom chart with latest template.
UPDATE public."chart_template" SET
    "template" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@using TNO.TemplateEngine

@{
  var datasets = ReportExtensions.GroupContent(SectionSettings.Dataset, Content, Sections, SectionSettings);
  var groups = ReportExtensions.GroupContent(SectionSettings.GroupBy, Content, Sections, SectionSettings);
  var labels = ReportExtensions.GetLabels(groups, SectionSettings, Sections);
  var datasetIndex = 0;
}
{
  "labels": [@String.Join(",", labels.Select(v => $"\"{v}\""))],
  "datasets": [@String.Join(", ", datasets.Select((ds) => {
    var index = datasetIndex++;
    var indexValue = SectionSettings.ApplyColorToValue ? -1 : index;
    var data = ReportExtensions.ExtractDatasetValues(SectionSettings.DatasetValue, SectionSettings.GroupBy, ds, labels);
    var backgroundColors = ReportExtensions.GetColors(SectionSettings.DatasetColors, indexValue, SectionSettings.Dataset, ds.Key, SectionSettings.DatasetValue);
    var borderColors = ReportExtensions.GetColors(SectionSettings.DatasetBorderColors, indexValue, SectionSettings.Dataset, ds.Key, SectionSettings.DatasetValue);

    return $"{{ " +
        $"\"label\": \"{ReportExtensions.GetLabel(ds, SectionSettings, Sections)}\", " +
        $"\"data\": [{String.Join(", ", data.Select(v => v.HasValue ? v.ToString() : "null"))}], " +
        $"\"backgroundColor\": {backgroundColors}, " +
        $"\"spanGaps\": true, " +
        $"\"borderWidth\": 2, " +
        $"\"minBarLength\": {SectionSettings.MinBarLength?.ToString() ?? "null"}, " +
        $"\"borderColor\": {borderColors} " +
      "}";
  }))]
}'
WHERE "name" = 'Custom';

END $$;
