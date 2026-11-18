DO $$
DECLARE ServiceAccountId INT;
DECLARE ReportId INT;
DECLARE ReportTemplateId INT;
DECLARE FolderFilterId INT;
DECLARE ReportSectionId INT;
DECLARE ChartTemplateId INT;
BEGIN

SELECT id INTO ServiceAccountId
FROM public."user" where username = 'service-account';

-- get the Report Template Id
SELECT INTO ReportTemplateId report_template_id FROM public."report" where "name" = 'Event of the Day' and owner_id = 1;

-- get the Chart Template Id for 'Topic Analysis - Topic Score'
SELECT INTO ChartTemplateId chart_template_id 
FROM public."report_template_chart_template" rtct 
JOIN public."chart_template" ct on rtct."chart_template_id" = ct."id" 
WHERE rtct.report_template_id = ReportTemplateId
AND ct.name = 'Topic Analysis - Topic Score';

-- Update the 'Topic Analysis - Topic Score' chart template
UPDATE public."chart_template"
SET "template" = 
'@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@{
  Content = Content.Where(x => x.Topics.All(a => a.Name != "Not Applicable"));
  var group = SectionSettings.GroupBy switch
  {
    "topicType" => Content.GroupBy(c => c.Topics.FirstOrDefault()?.TopicType.ToString() ?? "0").OrderByDescending(group => group.Key),
    "topicName" => Content.GroupBy(c => c.Topics.FirstOrDefault()?.Name ?? "0").OrderBy(group => group.Key),
    _ =>  Content.GroupBy(c => c.OtherSource).OrderBy(group => group.Key),
  };
  var sumOfAllScores = Content.Sum(c => c.Topics.FirstOrDefault()?.Score ?? 0);
}
{
  "labels": [@String.Join(",", group .Select(c => $"\"{c.Key}\""))],
  "datasets": [
    {
      "label": "Totals",
      "data":  [@String.Join(", ", group.Select(c => string.Format("{0:F2}", ((decimal)c.Sum(cc => cc.Topics.FirstOrDefault()?.Score ?? 0) / (decimal)sumOfAllScores) * 100)))],
      "backgroundColor": ["#006600","#BB1111"]
    }
   ]
}'
WHERE id = ChartTemplateId;

-- get the Chart Template Id for 'Topic Type - last 28 days' chart template
SELECT INTO ChartTemplateId chart_template_id 
FROM public."report_template_chart_template" rtct 
JOIN public."chart_template" ct on rtct."chart_template_id" = ct."id" 
WHERE rtct.report_template_id = ReportTemplateId
AND ct.name = 'Topic Type - last 28 days';

-- Update the 'Topic Type - last 28 days' chart template
UPDATE public."chart_template"
SET "template" = 
'@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@{
    // create a dictionary of dictionaries to flatten our data to something that can be accessed like this:
    // topicTypeAndCountByDate[date][topicType] 
    Dictionary<DateTime, Dictionary<string, int>> topicTypeAndCountByDate = Content
         .Where(x => x.Topics.All(a => a.Name != "Not Applicable"))
         .GroupBy(i => i.PublishedOn.HasValue ? new DateTime(i.PublishedOn!.Value.Year, i.PublishedOn!.Value.Month, i.PublishedOn!.Value.Day) : DateTime.MinValue)
         .ToDictionary(
             g => g.Key,
             g => g.GroupBy(gg => gg.Topics.Any() ? gg.Topics.FirstOrDefault()!.TopicType.ToString() : "N/A")
                 .ToDictionary(
                     ggg => ggg.Key,
                     ggg => ggg.ToList().Count()));

     // get a list of all the distinct dates
     DateTime[] dates = topicTypeAndCountByDate.Keys.OrderBy(x => x.ToUniversalTime()).ToArray();
     
     // create a dictionary to store a value per day for each topic type
     Dictionary<string, List<int>> topicTypeDataSets = new Dictionary<string, List<int>>
     {
         {"Proactive", new List<int>()},
         {"Issues", new List<int>()}
     };
     // for each date in the returned dataset
     foreach (var date in dates)
     {
         // iterate over the possible topic type hits on a day
         foreach(var topicType in topicTypeDataSets.Keys)
         {
             int topicTypeHits = 0;
             // get the target topic type hits for the day, or use ZERO if not found
             if (topicTypeAndCountByDate[date].ContainsKey(topicType))
             {
                 topicTypeHits = topicTypeAndCountByDate[date][topicType];
             }
             topicTypeDataSets[topicType].Add(topicTypeHits);
         };
     };
     var topicTypeColorLookup = new Dictionary<string, string>
     {
         { "Proactive", "#006600" },
         { "Issues", "#BB1111" }
     };
     int index = 0;
     int topicTypeCount = topicTypeDataSets.Keys.ToArray().Length;
}
{
  "labels": [@String.Join(",", dates.Select(x => $"\"{x:dd-MMM}\""))],
  "datasets": [
    @foreach (var topicType in topicTypeDataSets)
    {
      @{
        var datasetColor = "";
        if (!topicTypeColorLookup.TryGetValue(topicType.Key, out datasetColor )) {
          datasetColor = "";
        }
      }

      @($"{{ \"label\": \"{topicType.Key}\", \"fill\": true, \"borderColor\": \"{@datasetColor}\",\"backgroundColor\": \"{@datasetColor}\", \"data\": [{@String.Join(",",topicType.Value)} ] }}")
      if (++index < topicTypeCount) @(",")
    }
   ]
}'
WHERE id = ChartTemplateId;

END $$;
