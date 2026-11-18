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
  'Pre-grouped by Report Section - Over time' -- name
  , 'designed to use with reports which have sections representing individual series of data, the group by option will then be represented as a series over time for the section' -- description
  , true -- is_enabled
  , true -- is_public
  , 0 -- sort_order
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
}' -- template
  , '{"groupBy": ["sentiment", "byline", "mediaType", "contentType", "source", "series"], "options": {"scales": {"x": {"ticks": {"stepSize": 1}}, "y": {"ticks": {"stepSize": 1}}}, "plugins": {"legend": {"title": {"text": "Pre-grouped by Report Section - Over time", "display": true}}, "datalabels": {"anchor": "center", "labels": {"title": null}}}}, "chartTypes": ["line", "bar"]}' -- settings
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT DO NOTHING;

END $$;
