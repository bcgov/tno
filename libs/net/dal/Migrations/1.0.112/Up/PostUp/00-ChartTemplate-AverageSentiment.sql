UPDATE public."chart_template"
SET "name" = 'Average Sentiment'
  , "template" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
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
        var negative = c.Where(c => c.TonePools.FirstOrDefault()?.Value < 0);
        var negativeAverage = negative.Any() ? Math.Round(negative.Average(c => c.TonePools.FirstOrDefault()?.Value ?? 0)) : 0;

        var neutral= c.Where(c => c.TonePools.FirstOrDefault()?.Value == 0);
        var neutralAverage = neutral.Any() ? Math.Round(neutral.Average(c => c.TonePools.FirstOrDefault()?.Value ?? 0)) : 0;

        var positive = c.Where(c => c.TonePools.FirstOrDefault()?.Value > 0);
        var positiveAverage = positive .Any() ? Math.Round(positive .Average(c => c.TonePools.FirstOrDefault()?.Value ?? 0)) : 0;

        return new {
          Label = k,
          Average = Math.Round(c.GroupBy(c => c.TonePools.FirstOrDefault()?.Value ?? 0).Average(c => c.Key)),
          Negative = negativeAverage,
          Neutral = neutralAverage,
          Positive = positiveAverage,
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
      "label": "Average",
      "data": [@String.Join(",", groups.Select(g => g.Average))],
      "minBarLength": 1
    }
   ]
}'
  , "settings" = '{"groupBy": ["source", "series", "byline", "contentType", "year", "dayMonthYear", "monthYear", "mediaType"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}}, "y": {"max": 5, "min": -5, "ticks": {"stepSize": 1}}}, "plugins": {"legend": {"title": {"text": "Average Sentiment", "display": false}, "display": true}, "datalabels": {"anchor": "center", "labels": {"title": null}}}}, "chartTypes": ["bar", "line"]}'::jsonb
WHERE "name" = 'Sentiment';
