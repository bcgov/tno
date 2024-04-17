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

-- get the Chart Template Id
SELECT INTO ChartTemplateId chart_template_id 
FROM public."report_template_chart_template" rtct 
JOIN public."chart_template" ct on rtct."chart_template_id" = ct."id" 
WHERE rtct.report_template_id = ReportTemplateId
AND ct.name = 'Topic Analysis';

-- Update the 'Topic Analysis' chart template
UPDATE public."chart_template"
SET "name" = 'Topic Analysis - Topic Score',
"description" = 'Pie chart that can be configured to use either Topic Type or Topic Name. Uses Topic Score to calculate distribution.',
"template" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@{
  var group = SectionSettings.GroupBy switch
  {
    "topicType" => Content.GroupBy(c => c.Topics.FirstOrDefault()?.TopicType.ToString() ?? "0") .OrderBy(group => group.Key),
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
      "backgroundColor": ["#BB1111", "#006600"]
    }
   ]
}'
WHERE id = ChartTemplateId;

END $$;
