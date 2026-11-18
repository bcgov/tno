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
  'Coverage Count' -- name
  , 'A chart that counts all content by Media Type and groups them by the selected property' -- description
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
}' -- template
  , '{"groupBy": ["reportSection"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}, "stacked": true}, "y": {"ticks": {"stepSize": 1}, "stacked": true}}, "plugins": {"legend": {"title": {"text": "Coverage Count", "display": false}}, "datalabels": {"anchor": "center", "labels": {"title": {"display": true}, "formatter": "(value) => { return value > 0 ? value : ''; }"}}}, "indexAxis": "y"}, "chartTypes": ["bar", "line"]}'::jsonb -- settings
  , true -- is_public
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
);
