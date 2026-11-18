DO $$
BEGIN

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
    "reportSection" => Content.GroupBy(c => c.SectionName).OrderBy(group => group.Key),
    _ =>  Content.GroupBy(c => c.OtherSource).OrderBy(group => group.Key),
  };
  var labels = SectionSettings.GroupBy != "reportSection" ? group.Select(c => c.Key) : group.Select(c => Sections.ContainsKey(c.Key) ? Sections[c.Key].Settings.Label : "");
}
{
  "labels": [@String.Join(",", labels.Select(value => $"\"{(String.IsNullOrWhiteSpace(value) ? "Other" : value)}\"" ))],
  "datasets": [
    {
      "label": "Totals",
      "data":  [@String.Join(", ", group.Select(c => c.Count()))]
    }
   ]
}'
, "settings" = '{"groupBy": ["source", "series", "byline", "sentiment", "contentType", "mediaType", "dayMonthYear", "monthYear", "year", "sentimentSimple", "reportSection"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}}, "y": {"ticks": {"stepSize": 1}}}, "plugins": {"colors": {"enabled": true}, "legend": {"title": {"text": "", "display": true}, "display": true}, "datalabels": {"anchor": "center", "labels": {"title": {"display": false}}}}, "indexAxis": "x"}, "chartTypes": ["bar", "line", "pie", "doughnut", "polarArea", "radar"]}'::jsonb
WHERE "name" = 'Count';

UPDATE public."chart_template"
SET "template" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
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
        "reportSection" => c.SectionName,
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

    var labels = SectionSettings.GroupBy != "reportSection" ? groups.Select(g => g.Label) : groups.Select(g => Sections.ContainsKey(g.Label) ? Sections[g.Label].Settings.Label : "");
}
{
  "labels": [@String.Join(",", labels .Select(value => $"\"{value}\""))],
  "datasets": [
    {
      "label": "Average",
      "data": [@String.Join(",", groups.Select(g => g.Average))],
      "minBarLength": 1
    }
   ]
}'
  , "settings" = '{"groupBy": ["source", "series", "byline", "contentType", "year", "dayMonthYear", "monthYear", "mediaType", "reportSection"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}}, "y": {"max": 5, "min": -5, "ticks": {"stepSize": 1}}}, "plugins": {"legend": {"title": {"text": "Average Sentiment", "display": false}, "display": true}, "datalabels": {"anchor": "center", "labels": {"title": null}}}}, "chartTypes": ["bar", "line"]}'::jsonb
WHERE "name" = 'Average Sentiment';

UPDATE public."chart_template"
SET "template" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
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
        "reportSection" => c.SectionName,
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

    var labels = SectionSettings.GroupBy != "reportSection" ? groups.Select(g => g.Label) : groups.Select(g => Sections.ContainsKey(g.Label) ? Sections[g.Label].Settings.Label : "");
}
{
  "labels": [@String.Join(",", labels.Select(value => $"\"{value}\""))],
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
}'
  , "settings" = '{"groupBy": ["mediaType", "source", "series", "byline", "sentiment", "sentimentSimple", "contentType", "dayMonthYear", "monthYear", "year", "reportSection"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}}, "y": {"ticks": {"stepSize": 1}}}, "plugins": {"legend": {"title": {"text": "Sentiment Count", "display": true}}, "datalabels": {"anchor": "center", "labels": {"title": null}}}}, "chartTypes": ["bar", "line"]}'::jsonb
WHERE "name" = 'Sentiment Count';

END $$;
