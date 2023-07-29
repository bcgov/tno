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
  'Count' -- name
  , 'A chart with number of stories on the x-axis.' -- description
  , true -- is_enabled
  , true -- is_public
  , 0 -- sort_order
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartTemplateModel>
@using System
@using System.Linq
@using TNO.Entities
@{
  var group = Settings.GroupBy switch
  {
    "product" => Content.GroupBy(c => c.Product?.Name).OrderBy(group => group.Key),
    "contentType" => Content.GroupBy(c => c.ContentType.ToString()).OrderBy(group => group.Key),
    "byline" => Content.GroupBy(c => c.Byline).OrderBy(group => group.Key),
    "series" => Content.GroupBy(c => c.SeriesId.ToString()).OrderBy(group => group.Key),
    "sentiment" => Content.GroupBy(c => c.TonePools.FirstOrDefault()?.Value.ToString() ?? "0").OrderBy(group => group.Key),
    "source" => Content.GroupBy(c => c.OtherSource).OrderBy(group => group.Key),
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
}' -- template
  , '{"graph": ["count", "sentiment", "earnedMedia"], "groupBy": ["contentType", "byline", "source", "series", "sentiment", "product"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}}}, "plugins": {"datalabels": {"anchor": "center", "labels": {"title": null}}}, "indexAxis": "y"}, "chartTypes": ["bar", "pie", "line", "doughnut", "scatter", "bubble", "polarArea", "radar"], "isOverTime": false, "groupBySection": ["contentType"]}' -- settings
  , '' -- created_by
  , '' -- updated_by
), (
  'Stacked Published On' -- name
  , 'A stacked chart with published dates on the x-axis.' -- description
  , true -- is_enabled
  , true -- is_public
  , 0 -- sort_order
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartTemplateModel>
@using System
@using System.Linq
@using TNO.Entities
@{
 // Get all dates in data
  var dates = Content
    .GroupBy(c => c.PublishedOn.HasValue ? new DateTime(c.PublishedOn!.Value.Year, c.PublishedOn!.Value.Month, c.PublishedOn!.Value.Day) : DateTime.MinValue)
    .OrderBy(g=> g.Key)
    .ToArray();

  // Get all groups in data
  var groups = Content
    .GroupBy(
      c => Settings.GroupBy switch {
        "product" => c.Product?.Name,
        "contentType" => c.ContentType.ToString(),
        "byline" => c.Byline,
        "series" => c.Series?.Name,
        "sentiment" => c.TonePools.FirstOrDefault()?.Value.ToString(),
        "source" => c.OtherSource,
        _ => c.OtherSource
      },
      c => c,
      (k, c) => new { Label = k, Group = c.GroupBy(c => c.PublishedOn.HasValue ? new DateTime(c.PublishedOn!.Value.Year, c.PublishedOn!.Value.Month, c.PublishedOn!.Value.Day) : DateTime.MinValue) })
    .OrderBy(g=> g.Label)
    .ToArray();
}
{
  "labels": [@String.Join(",", dates.Select(c => $"\"{c.Key:dd-MMM}\""))],
  "datasets": [
    @for (var i = 0; i < groups.Length; i++)
    {
      @($"{{ \"label\": \"{groups[i].Label}\", \"data\": [{@String.Join(",", dates.Select(d => groups[i].Group.Where(g => g.Key == d.Key).Count()))} ] }}")
      if (i < groups.Length-1) @(",")
    }
   ]
}' -- template
  , '{"graph": ["count"], "groupBy": ["source", "contentType", "byline", "series", "sentiment", "product"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}, "stacked": true}, "y": {"ticks": {"stepSize": 1}, "stacked": true}}, "plugins": {"datalabels": {"anchor": "center", "labels": {"title": null}}}}, "chartTypes": ["bar", "line", "pie", "doughnut", "scatter", "bubble", "polarArea", "radar"]}' -- settings
  , '' -- created_by
  , '' -- updated_by
), (
  'Sentiment' -- name
  , 'A chart with sentiment on the x-axis.' -- description
  , true -- is_enabled
  , true -- is_public
  , 0 -- sort_order
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartTemplateModel>
@using System
@using System.Linq
@using TNO.Entities
@{
 // Create the array of sentiment
  var sentiment = new [] { -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5 };

  // Get all groups in data
  var groups = Content
    .GroupBy(
      c => Settings.GroupBy switch
      {
        "product" => c.Product?.Name,
        "contentType" => c.ContentType.ToString(),
        "byline" => c.Byline,
        "series" => c.Series?.Name,
        "source" => c.OtherSource,
        _ => c.OtherSource,
      },
      c => c,
      (k, c) => new {
        Label = k,
        Average = c.GroupBy(c => c.TonePools.FirstOrDefault()?.Value ?? 0).Average(c => c.Key),
        Group = c.GroupBy(c => c.TonePools.FirstOrDefault()?.Value ?? 0)
      })
    .OrderBy(g=> g.Label)
    .ToArray();
}
{
  "labels": [@String.Join(",", groups.Select(g => $"\"{g.Label}\""))],
  "datasets": [
    {
      "label": "Negative",
      "data": [@String.Join(",", groups.Select(g => g.Average < 0 ? Math.Round(g.Average) : 0))],
      "backgroundColor": "red"
    },
    {
      "label": "Positive",
      "data": [@String.Join(",", groups.Select(g => g.Average > 0 ? Math.Round(g.Average) : 0))],
      "backgroundColor": "green"
    }
   ]
}' -- template
  , '{"graph": [], "groupBy": ["product", "source", "series", "byline", "contentType"], "options": {"scales": {"x": {"max": 5, "min": -5, "ticks": {"stepSize": 1}, "stacked": true}, "y": {"ticks": {"stepSize": 1}, "stacked": true}}, "plugins": {"datalabels": {"anchor": "center", "labels": {"title": null}}}, "indexAxis": "y"}, "chartTypes": ["bar", "line", "pie", "doughnut", "polarArea", "radar"]}' -- settings
  , '' -- created_by
  , '' -- updated_by
), (
  'Earned Media' -- name
  , 'A chart with earned media on the x-axis.' -- description
  , false -- is_enabled
  , true -- is_public
  , 0 -- sort_order
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartTemplateModel>
@using System
@using System.Linq
@using TNO.Entities
@{
  var group = Settings.GroupBy switch
  {
    "product" => Content.GroupBy(c => c.Product?.Name).OrderBy(group => group.Key),
    "contentType" => Content.GroupBy(c => c.ContentType.ToString()).OrderBy(group => group.Key),
    "byline" => Content.GroupBy(c => c.Byline).OrderBy(group => group.Key),
    "series" => Content.GroupBy(c => c.SeriesId.ToString()).OrderBy(group => group.Key),
    "sentiment" => Content.GroupBy(c => c.TonePools.FirstOrDefault()?.Value.ToString() ?? "0").OrderBy(group => group.Key),
    "source" => Content.GroupBy(c => c.OtherSource).OrderBy(group => group.Key),
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
}' -- template
  , '{"graph": [], "groupBy": ["product", "series", "source", "byline", "contentType", "sentiment"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}}, "y": {"ticks": {"stepSize": 1}}}, "plugins": {"datalabels": {"anchor": "center", "labels": {"title": null}}}, "indexAxis": "y"}, "chartTypes": ["bar", "line", "pie", "doughnut", "polarArea", "radar"]}' -- settings
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT DO NOTHING;

END $$;
