-- Restore the removed chart templates as seeded. A template the migration kept is left unchanged.

INSERT INTO public."chart_template" (
  "name"
  , "description"
  , "template"
  , "is_enabled"
  , "sort_order"
  , "is_public"
  , "settings"
  , "created_by"
  , "updated_by"
) VALUES (
  'Count'
  , 'A chart with number of stories on the x-axis.'
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
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
  , true
  , 0
  , true
  , '{"groupBy": ["source", "series", "byline", "sentiment", "contentType", "mediaType", "dayMonthYear", "monthYear", "year", "sentimentSimple", "reportSection"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}}, "y": {"ticks": {"stepSize": 1}}}, "plugins": {"colors": {"enabled": true}, "legend": {"title": {"text": "", "display": true}, "display": true}, "datalabels": {"anchor": "center", "labels": {"title": {"display": false}}}}, "indexAxis": "x"}, "chartTypes": ["bar", "line", "pie", "doughnut", "polarArea", "radar"]}'::jsonb
  , ''
  , ''
) ON CONFLICT ("name") DO NOTHING;

INSERT INTO public."chart_template" (
  "name"
  , "description"
  , "template"
  , "is_enabled"
  , "sort_order"
  , "is_public"
  , "settings"
  , "created_by"
  , "updated_by"
) VALUES (
  'Stacked Published On'
  , 'A stacked chart with published dates on the x-axis.'
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
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
      c => SectionSettings.GroupBy switch {
        "mediaType" => c.MediaType?.Name,
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
}'
  , true
  , 0
  , true
  , '{"graph": ["count"], "groupBy": ["source", "contentType", "byline", "series", "sentiment", "product"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}, "stacked": true}, "y": {"ticks": {"stepSize": 1}, "stacked": true}}, "plugins": {"datalabels": {"anchor": "center", "labels": {"title": null}}}}, "chartTypes": ["bar", "line", "pie", "doughnut", "scatter", "bubble", "polarArea", "radar"]}'::jsonb
  , ''
  , ''
) ON CONFLICT ("name") DO NOTHING;

INSERT INTO public."chart_template" (
  "name"
  , "description"
  , "template"
  , "is_enabled"
  , "sort_order"
  , "is_public"
  , "settings"
  , "created_by"
  , "updated_by"
) VALUES (
  'Average Sentiment'
  , 'A chart with sentiment on the x-axis.'
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
  , true
  , 0
  , true
  , '{"groupBy": ["source", "series", "byline", "contentType", "year", "dayMonthYear", "monthYear", "mediaType", "reportSection"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}}, "y": {"max": 5, "min": -5, "ticks": {"stepSize": 1}}}, "plugins": {"legend": {"title": {"text": "Average Sentiment", "display": false}, "display": true}, "datalabels": {"anchor": "center", "labels": {"title": null}}}}, "chartTypes": ["bar", "line"]}'::jsonb
  , ''
  , ''
) ON CONFLICT ("name") DO NOTHING;

INSERT INTO public."chart_template" (
  "name"
  , "description"
  , "template"
  , "is_enabled"
  , "sort_order"
  , "is_public"
  , "settings"
  , "created_by"
  , "updated_by"
) VALUES (
  'Earned Media'
  , 'A chart with earned media on the x-axis.'
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
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
}'
  , false
  , 0
  , true
  , '{"graph": [], "groupBy": ["product", "series", "source", "byline", "contentType", "sentiment"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}}, "y": {"ticks": {"stepSize": 1}}}, "plugins": {"datalabels": {"anchor": "center", "labels": {"title": null}}}, "indexAxis": "y"}, "chartTypes": ["bar", "line", "pie", "doughnut", "polarArea", "radar"]}'::jsonb
  , ''
  , ''
) ON CONFLICT ("name") DO NOTHING;

INSERT INTO public."chart_template" (
  "name"
  , "description"
  , "template"
  , "is_enabled"
  , "sort_order"
  , "is_public"
  , "settings"
  , "created_by"
  , "updated_by"
) VALUES (
  'Sentiment Count'
  , 'A chart that counts all the negative, neutral, and positive stories and groups them by the selected property'
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
  , true
  , 0
  , true
  , '{"groupBy": ["mediaType", "source", "series", "byline", "sentiment", "sentimentSimple", "contentType", "dayMonthYear", "monthYear", "year", "reportSection"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}}, "y": {"ticks": {"stepSize": 1}}}, "plugins": {"legend": {"title": {"text": "Sentiment Count", "display": true}}, "datalabels": {"anchor": "center", "labels": {"title": null}}}}, "chartTypes": ["bar", "line"]}'::jsonb
  , ''
  , ''
) ON CONFLICT ("name") DO NOTHING;

INSERT INTO public."chart_template" (
  "name"
  , "description"
  , "template"
  , "is_enabled"
  , "sort_order"
  , "is_public"
  , "settings"
  , "created_by"
  , "updated_by"
) VALUES (
  'Coverage Count'
  , 'A chart that counts all content by Media Type and groups them by the selected property'
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using System.Linq
@using System.Text.Json
@using TNO.Entities
@{
    var UnknownMediaTypeLabel = "Unknown Media Type";
    List<string> allMediaTypes = Content.Select(m => m.MediaType?.Name ?? UnknownMediaTypeLabel).Distinct().ToList();
    // Create a placeholder Dictionary with all known Media Types as the Key and an empty array to store the count of mentions by "Group By"
    Dictionary<string, List<int>> mentionsByMediaType = new Dictionary<string, List<int>>();
    allMediaTypes.ForEach(mt => mentionsByMediaType.Add(mt, new List<int>()));

    // Get all groups in data
    var groups = Content
      .GroupBy(
        c => SectionSettings.GroupBy switch
        {
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
          Dictionary<string, int> groupMentionsByMediaType = c.GroupBy(g => g.MediaType?.Name ?? UnknownMediaTypeLabel)
            .ToDictionary( gdc => gdc.Key, gdc => gdc.Count());
        
          // iterate over each Media Type that was found in the collective Content
          foreach(string mediaType in allMediaTypes ) {
            // if this grouping contains the current Media Type, add it to the master list
            if (groupMentionsByMediaType.ContainsKey(mediaType)) {
              mentionsByMediaType[mediaType].Add(groupMentionsByMediaType[mediaType]);
            } else {
              // no mention for this Media Type in this grouping, pad the array with a 0
              mentionsByMediaType[mediaType].Add(0);
            }
          }

        return new {
          Label = SectionSettings.GroupBy != "reportSection" ? k : Sections.ContainsKey(k) ? Sections[k].Settings.Label : "",
          MentionsByMediaType = groupMentionsByMediaType
        };
      })
      .OrderBy(g=> g.Label)
      .ToArray();
      
      // build up the objects to serialize for the chart data
     var labels = groups.Select(value => value.Label);
     var datasets = mentionsByMediaType.Select(m => 
       new {
         label = m.Key
         ,data = m.Value
         //,backgroundColor = TODO: map Media Type to fixed color
       }
     );
}
@* now render the JSON for the chart data*@
{
  "labels": @JsonSerializer.Serialize(labels) ,
  "datasets": @JsonSerializer.Serialize(datasets) 
   @* ,"rawContent": @.JsonSerializer.Serialize(Content) *@
   @* ,"rawGroups": @JsonSerializer.Serialize(groups) *@
   @* ,"rawSections": @JsonSerializer.Serialize(Sections)  *@
   @* ,"rawMentionsByMediaType": @JsonSerializer.Serialize(mentionsByMediaType) *@
   @* ,"rawDatasets": @JsonSerializer.Serialize(datasets) *@
}'
  , true
  , 0
  , true
  , '{"groupBy": ["reportSection"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}, "stacked": true}, "y": {"ticks": {"stepSize": 1}, "stacked": true}}, "plugins": {"legend": {"title": {"text": "Coverage Count", "display": false}}, "datalabels": {"anchor": "center", "labels": {"title": {"display": true}, "formatter": "(value) => { return value > 0 ? value : ''; }"}}}, "indexAxis": "y"}, "chartTypes": ["bar", "line"]}'::jsonb
  , ''
  , ''
) ON CONFLICT ("name") DO NOTHING;

INSERT INTO public."chart_template" (
  "name"
  , "description"
  , "template"
  , "is_enabled"
  , "sort_order"
  , "is_public"
  , "settings"
  , "created_by"
  , "updated_by"
) VALUES (
  'Pre-grouped by Report Section - Over time'
  , 'designed to use with reports which have sections representing individual series of data, the group by option will then be represented as a series over time for the section'
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using System.Linq
@using System.Text.Json
@using TNO.Entities
@{
  // to be consistent, with adding data to series, we need to know all the possible dates used in all content
  var allDates = Content
    .Select(c => new DateTime(c.PublishedOn.Value.Year, c.PublishedOn.Value.Month,c.PublishedOn.Value.Day))
    .Distinct().ToList();
    allDates .Sort((x, y) => DateTime.Compare(x, y)); 
  var sectionGroups = Content
    .GroupBy(c => c.SectionName,
          c => c,
      (key, sc) => {
        // group the content for the section by publishedOn - Date only
        var sectionContentByDate = sc
          .GroupBy(dg=> new DateTime(dg.PublishedOn.Value.Year, dg.PublishedOn.Value.Month,dg.PublishedOn.Value.Day))
          .ToDictionary(g => g.Key, g => g.ToArray());
          var seriesData = new List<double?>();
          foreach(var groupDate in allDates) {
            double? seriesVal = null;
            if (sectionContentByDate.ContainsKey(groupDate)) {
              switch(SectionSettings.GroupBy) {
                case "mediaType":
                  seriesVal = sectionContentByDate[groupDate].GroupBy(c => c.MediaType?.Name.ToString()).Count();
                  break;
                case "contentType":
                  seriesVal = sectionContentByDate[groupDate].GroupBy(c => c.ContentType.ToString()).Count();
                  break;
                case "byline":
                  seriesVal = sectionContentByDate[groupDate].GroupBy(c => c.Byline).Count();
                  break;
                case "series":
                  seriesVal = sectionContentByDate[groupDate].GroupBy(c => c.Series?.Name).Count();
                  break;
                case "source": 
                  seriesVal = sectionContentByDate[groupDate].GroupBy(c => c.OtherSource).Count();
                  break;
                case "sentiment":
                  seriesVal = Math.Round(sectionContentByDate[groupDate].GroupBy(c => c.TonePools.FirstOrDefault()?.Value ?? 0).Average(c => c.Key));
                  break;
                }
              }
              seriesData.Add(seriesVal);
            }
       
            return new {
              Label = sc.Select(l => l.SectionLabel).First(),
              // GroupByDate = sectionContentByDate,
              SeriesData = seriesData
            };
        });
        
        // create some models and populate them for the json output
        var labels = allDates.Select(date => $"{date:dd-MM-yyyy}");
        var datasets = sectionGroups.Select(sg => 
           new {
             label = sg.Label
             ,data = sg.SeriesData
           }
       );
  }
@* render models as json for chart or for debugging *@
{
  "labels": @(JsonSerializer.Serialize(labels)),
  "datasets": @(JsonSerializer.Serialize(datasets))
  @* "rawSectionGroups": @(JsonSerializer.Serialize(sectionGroups)) *@
  @* "rawAllDates": @(JsonSerializer.Serialize(allDates )) *@
}'
  , true
  , 0
  , true
  , '{"groupBy": ["sentiment", "byline", "mediaType", "contentType", "source", "series"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}}, "y": {"ticks": {"stepSize": 1}}}, "plugins": {"legend": {"title": {"text": "Pre-grouped by Report Section - Over time", "display": true}}, "datalabels": {"anchor": "center", "labels": {"title": null}}}}, "chartTypes": ["line", "bar"]}'::jsonb
  , ''
  , ''
) ON CONFLICT ("name") DO NOTHING;
