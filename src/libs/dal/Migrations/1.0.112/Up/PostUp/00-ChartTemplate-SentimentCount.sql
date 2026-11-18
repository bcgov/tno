INSERT INTO public."chart_template" (
  "name"
  , "description"
  , "template"
  , "settings"
  , "is_public"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'Sentiment Count' -- name
  , 'A chart that counts all the negative, neutral, and positive stories and groups them by the selected property' -- description
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@{
 // Create the array of sentiment
  var sentiment = new [] { -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5 };

  // Get all groups in data
  var groups = Content
    .GroupBy(
      c => SectionSettings.GroupBy switch
      {
        "mediaType" => c.MediaType?.Name,
        "contentType" => c.ContentType.ToString(),
        "byline" => c.Byline,
        "series" => c.Series?.Name,
        "source" => c.OtherSource,
        "dayMonthYear" => $"{c.PublishedOn:dd-MM-yyyy}",
        "monthYear" => $"{c.PublishedOn:MM-yyyy}",
        "year" => $"{c.PublishedOn:yyyy}",
        "sentiment" => $"{c.TonePools.FirstOrDefault()?.Value ?? 0}",
        "sentimentSimple" => (c.TonePools.FirstOrDefault()?.Value ?? 0) < 0 ? "Negative" : (c.TonePools.FirstOrDefault()?.Value ?? 0) > 0 ? "Positive": "Neutral",
        _ => c.OtherSource,
      },
      c => c,
      (k, c) => {
        var negative = c.Where(c => (c.TonePools.FirstOrDefault()?.Value ?? 0) < 0).Count();
        var neutral = c.Where(c => (c.TonePools.FirstOrDefault()?.Value ?? 0) == 0).Count();
        var positive= c.Where(c => (c.TonePools.FirstOrDefault()?.Value ?? 0) > 0).Count();

        return new {
          Label = k,
          Average = c.GroupBy(c => c.TonePools.FirstOrDefault()?.Value ?? 0).Average(c => c.Key),
          Negative= negative ,
          Neutral= neutral ,
          Positive = positive,
          Group = c.GroupBy(c => c.TonePools.FirstOrDefault()?.Value ?? 0)
      };
      })
      .OrderBy(g=> g.Label)
      .ToArray();
}
{
  "labels": [@String.Join(",", groups.Select(g => $"\"{g.Label}\""))],
  "datasets": [
    {
      "label": "Negative",
      "data": [@String.Join(",", groups.Select(g => g.Negative))],
      "backgroundColor": "red"
    },
    {
      "label": "Neutral",
      "data": [@String.Join(",", groups.Select(g => g.Neutral))],
      "backgroundColor": "grey"
    },
    {
      "label": "Positive",
      "data": [@String.Join(",", groups.Select(g => g.Positive))],
      "backgroundColor": "green"
    }
   ]
}' -- template
  , '{"groupBy": ["mediaType", "source", "series", "byline", "sentiment", "sentimentSimple", "contentType", "dayMonthYear", "monthYear", "year"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}}, "y": {"ticks": {"stepSize": 1}}}, "plugins": {"legend": {"title": {"text": "Sentiment Count", "display": true}}, "datalabels": {"anchor": "center", "labels": {"title": null}}}}, "chartTypes": ["bar", "line"]}'::jsonb -- settings
  , true -- is_public
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
);
