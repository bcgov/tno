DO $$
DECLARE ReportTemplateId INT;
BEGIN

-- get the Report Template Id
SELECT INTO ReportTemplateId report_template_id FROM public."report" where "name" = 'Event of the Day' and owner_id = 1;

UPDATE public.report_template SET
  "body" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.TemplateEngine.Models.Reports.ReportEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@using TNO.TemplateEngine
@{
  var pageBreak = Settings.Sections.UsePageBreaks ? "page-break-after: always;" : "";
  var utcOffset = ReportExtensions.GetUtcOffset(System.DateTime.Now, "Pacific Standard Time");
}
<h1 id="top" style="margin: 0; padding: 0;">@Settings.Subject.Text</h1>
<a name="top"></a>
<div style="color:red;">DO NOT FORWARD THIS EMAIL TO ANYONE</div>
<br/>

@if (Content.Count() == 0)
{
  <p>There is no content in this report.</p>
}
@if (ViewOnWebOnly)
{
  <a href="@SubscriberAppUrl" target="_blank">Click to view this report online</a>
}
else
{
  var contentCount = 0;
  var allContent = Content.ToArray();
  foreach (var section in Sections)
  {
    var sectionContent = section.Value.Content.ToArray();

    if (!section.Value.IsEnabled) continue;
    if (sectionContent.Length == 0 && section.Value.Settings.HideEmpty) continue;

    <hr style="width: 100%; display: inline-block;" />
    <div style="@pageBreak">
      @if (!String.IsNullOrEmpty(section.Value.Settings.Label))
      {
        <h2 id="section-@section.Value.Id" styled="margin: 0; padding: 0;">@section.Value.Settings.Label<a name="section-@section.Value.Id"></a></h2>
      }
      @if (!String.IsNullOrEmpty(section.Value.Description))
      {
        <p>@section.Value.Description</p>
      }

      @if (section.Value.SectionType == ReportSectionType.TableOfContents)
      {
        @* Table of Contents Section *@
        <p>No Template defined for: TableOfContents</p>
        continue;
      }
      @if (section.Value.SectionType == ReportSectionType.Text)
      {
        @* Text Section *@
        <p>No Template defined for: Text</p>
        continue;
      }
      @if (section.Value.SectionType == ReportSectionType.Gallery)
      {
        @* Gallery Section *@
        <p>No Template defined for: Gallery</p>
        continue;
      }

      @* Content Section *@
      @if (section.Value.SectionType == ReportSectionType.Content)
      {
          Dictionary<string, Dictionary<string,int>> topicScoresByTopicTypeAndName;
          Dictionary<string, int> totalScoresByTopicType;
          if (section.Value.Aggregations != null) {
            topicScoresByTopicTypeAndName = section.Value.Aggregations.GetTopicScoresByTopicTypeAndName();
            totalScoresByTopicType = section.Value.Aggregations.GetTotalScoresByTopicType();
          } else {
            topicScoresByTopicTypeAndName = sectionContent.GetTopicScoresByTopicTypeAndName();
            totalScoresByTopicType = sectionContent.GetTotalScoresByTopicType();
          }
          int sumOfAllTopicScores = totalScoresByTopicType.Sum(s => s.Value);
              <ul style="margin:0; margin-left: 25px; padding:0;">
              @foreach(KeyValuePair<string, Dictionary<string,int>> topicTypeGrouping in topicScoresByTopicTypeAndName.OrderByDescending(t => t.Key) )
              {
                var listItemColorStyle = ReportExtensions.GetColorFromName(topicTypeGrouping.Key, new string[] {"Issues","Proactive"}, new string[] {"#BB1111", "#006600"});
                var sumOfAllTopicTypeScores = totalScoresByTopicType[topicTypeGrouping.Key];
                var rootAggregationAsPercentage = (sumOfAllTopicTypeScores/(decimal)sumOfAllTopicScores * 100);
                <li style="font-weight:bold; @listItemColorStyle " >@topicTypeGrouping.Key (@String.Format("{0:F2}", @rootAggregationAsPercentage)%)</li>
                <ul>
                  @foreach (var topicsForTopicType in topicTypeGrouping.Value.OrderByDescending(t => t.Value))
                  {
                    var childAggregationAsPercentage = (topicsForTopicType.Value/(decimal)sumOfAllTopicScores * 100);
                    <li style="@listItemColorStyle">@topicsForTopicType.Key (@String.Format("{0:F2}", @childAggregationAsPercentage )%)</li>
                   }
                </ul>
              }
              </ul>
        
        @* Full Stories *@
        @if (section.Value.Settings.ShowFullStory)
        {
          <p>No Template defined for: ShowFullStory</p>
        }
      }
      
      @if (section.Value.SectionType == ReportSectionType.MediaAnalytics)
      {
        @* Media Analytics Section *@
        foreach (var chart in section.Value.ChartTemplates)
        {
          <img alt="@chart.SectionSettings.AltText" src="@chart.Uid" />
        }
      }
    </div>
  }
}

<p style="font-size: 9pt; margin-top: 0.5rem;">
  Terms of Use - This summary is a service provided by Government Communications and Public Engagement and is only intended for original addressee. All content is the copyrighted property of a third party creator of the material.
  Copying, retransmitting, archiving, redistributing, selling, licensing, or emailing the material to any third party or any employee of the Province who is not authorized to access the material is prohibited.
</p>
'
WHERE "id" = ReportTemplateId;

END $$;
