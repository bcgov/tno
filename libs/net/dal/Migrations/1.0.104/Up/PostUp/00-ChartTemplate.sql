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
  'Topic Analysis' --name
  ,'Pie chart that can be configured to use either Topic Type or Topic Name' --description
  , true -- is_enabled
  , true -- is_public
  , 0 -- sort_order
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ChartEngineContentModel>
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
}' -- template
, '{
  "groupBy": [
    "topicType",
    "topicName"
  ],
  "options": {
    "plugins": {
      "legend": {
        "align": "center",
        "title": {
          "text": "WIP | Topic Analysis"
        },
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
        },
        "formatter": "(value, ctx) => { const datapoints = ctx.chart.data.datasets[0].data; const total = datapoints.reduce((total, datapoint) => total + datapoint, 0); const percentage = value / total * 100; return percentage.toFixed(2) + ''%''; }"
      }
    }
  },
  "chartTypes": [
    "pie",
    "doughnut"
  ]
}' -- settings
, '' -- created_by
, ''); -- updated_by

END $$;
