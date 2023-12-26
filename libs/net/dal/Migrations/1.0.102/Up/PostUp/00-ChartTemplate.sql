DO $$
BEGIN

UPDATE public."chart_template"
SET
  "template" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@{
  var group = SectionSettings.GroupBy switch
  {
    "topicType" => Content.GroupBy(c => c.Topics.FirstOrDefault()?.TopicType.ToString() ?? "0").OrderBy(group => group.Key),
    "topicName" => Content.GroupBy(c => c.Topics.FirstOrDefault()?.Name ?? "0").OrderBy(group => group.Key),
    _ =>  Content.GroupBy(c => c.OtherSource).OrderBy(group => group.Key),
  };
}
{
  "labels": [@String.Join(",", group .Select(c => $"\"{c.Key}\""))],
  "datasets": [
    {
      "label": "Totals",
      "data":  [@String.Join(", ", group .Select(c => string.Format("{0:F1}",  ((decimal)c.Count())/Content.Count() * 100)))],
      "backgroundColor": ["#BB1111", "#006600"]
    }
   ]
}',
"settings" = '{
  "plugins": {
    "legend": {
      "align": "center",
      "labels": {
        "font": {
          "size": 18
        }
      },
      "display": true,
      "position": "bottom"
    },
    "datalabels": {
      "font": {
        "size": 18
      },
      "color": "#fff",
      "anchor": "center",
      "labels": {
        "font": {
          "size": 24
        },
        "title": {
          "display": true
        }
      }
    }
  }
}'
WHERE "name" = 'Count';

END $$;
