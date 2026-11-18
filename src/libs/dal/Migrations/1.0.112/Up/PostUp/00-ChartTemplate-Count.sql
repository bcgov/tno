UPDATE public."chart_template"
SET "template" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@{
  var group = SectionSettings.GroupBy switch
  {
    "mediaType" => Content.GroupBy(c => c.MediaType?.Name).OrderBy(group => group.Key),
    "contentType" => Content.GroupBy(c => c.ContentType.ToString()).OrderBy(group => group.Key),
    "byline" => Content.GroupBy(c => c.Byline).OrderBy(group => group.Key),
    "series" => Content.GroupBy(c => c.SeriesId.ToString()).OrderBy(group => group.Key),
    "sentiment" => Content.GroupBy(c => c.TonePools.FirstOrDefault()?.Value.ToString() ?? "0").OrderBy(group => group.Key),
    "sentimentSimple" => Content.GroupBy(c => {
      var value = c.TonePools.FirstOrDefault()?.Value ?? 0;
      if (value  < 0) return "Negative";
      if (value > 0) return "Positive";
      return "Neutral";
    }).OrderBy(group => group.Key),
    "source" => Content.GroupBy(c => c.OtherSource).OrderBy(group => group.Key),
    "dayMonthYear" => Content.GroupBy(c => $"{c.PublishedOn:dd-MM-yyyy}").OrderBy(group => group.Key),
    "monthYear" => Content.GroupBy(c =>$"{c.PublishedOn:MM-yyyy}").OrderBy(group => group.Key),
    "year" => Content.GroupBy(c => $"{c.PublishedOn:yyyy}").OrderBy(group => group.Key),
    _ =>  Content.GroupBy(c => c.OtherSource).OrderBy(group => group.Key),
  };
}
{
  "labels": [@String.Join(",", group .Select(c => $"\"{c.Key}\""))],
  "datasets": [
    {
      "label": "Totals",
      "data":  [@String.Join(", ", group .Select(c => c.Count()))]
    }
   ]
}'
  , "settings" = '{"groupBy": ["source", "series", "byline", "sentiment", "contentType", "mediaType", "dayMonthYear", "monthYear", "year", "sentimentSimple"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}}, "y": {"ticks": {"stepSize": 1}}}, "plugins": {"colors": {"enabled": true}, "legend": {"title": {"text": "", "display": true}, "display": true}, "datalabels": {"anchor": "center", "labels": {"title": {"display": true}}}}, "indexAxis": "x"}, "chartTypes": ["bar", "line", "pie", "doughnut", "polarArea", "radar"]}'::jsonb
WHERE "name" = 'Count';
