DO $$
BEGIN

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
    if (section.Value.IsEnabled &&
      (sectionContent.Length > 0 ||
      !section.Value.Settings.HideEmpty ||
      (section.Value.Settings.SectionType == ReportSectionType.TableOfContents)
        && Content.Any()))
    {
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
        @* Charts *@
        @if (section.Value.Settings.ChartsOnTop && section.Value.Settings.ShowCharts)
        {
          foreach (var chart in section.Value.ChartTemplates)
          {
            <img alt="@chart.SectionSettings.AltText" src="@chart.Uid" />
          }
        }
        @if (section.Value.Settings.SectionType == ReportSectionType.Content)
        {
          @* Section Table of Contents *@
          var positionInTOC = 0;
          if (section.Value.Settings.ShowHeadlines)
          {
              @if (section.Value.Settings.GroupBy != "")
              {
                var contentGroupings = sectionContent.GetContentGroupings(section.Value.Settings.GroupBy);
                <ul style="margin:0; margin-left: 25px; padding:0;">
                @foreach(KeyValuePair<string, List<long>> contentGroup in contentGroupings)
                {
                <li>@section.Value.Settings.GroupBy = @contentGroup.Key</li>
                  <ul style="margin:0; margin-left: 25px; padding:0;">
                    @foreach(long contentId in contentGroup.Value)
                    {
                      var content = sectionContent.FirstOrDefault(c => c.Id == contentId);
                      var headlineLink = contentCount + positionInTOC;
                      positionInTOC++;
                      if (section.Value.Settings.ShowFullStory)
                      {
                        <li>@ReportExtensions.GetFullHeadline(content, Model, utcOffset, true, $"#item-{headlineLink}")</li>
                      }
                      else
                      {
                        <li>@ReportExtensions.GetFullHeadline(content, Model, utcOffset, true, "", "_blank")</li>
                      }
                    }
                  </ul>
                }
                </ul>
              }
              else
              {
                <ul style="margin:0; margin-left: 25px; padding:0;">
                @foreach (var content in sectionContent)
                {
                  var headlineLink= contentCount + positionInTOC;
                  positionInTOC++;
                    @if (section.Value.Settings.ShowFullStory)
                    {
                      <li>@ReportExtensions.GetFullHeadline(content, Model, utcOffset, true, $"#item-{headlineLink}")</li>
                    }
                    else
                    {
                      <li>@ReportExtensions.GetFullHeadline(content, Model, utcOffset, true, "", "_blank")</li>
                    }
                }
                </ul>
              }
          }

          @* Full Stories *@
          @if (section.Value.Settings.ShowFullStory)
          {
            for (var i = 0; i < sectionContent.Length; i++)
            {
              var content = sectionContent[i];
              var rawHeadline = ReportExtensions.GetHeadline(content, Model);
              var headline = Settings.Content.HighlightKeywords ? ReportExtensions.HighlightKeyWords(section.Value.Filter, rawHeadline, "headline")  : rawHeadline;
              var rawBody = ReportExtensions.GetBody(content, Model);
              var body = Settings.Content.HighlightKeywords ? ReportExtensions.HighlightKeyWords(section.Value.Filter, rawBody, "body")  : rawBody;
              var rawByline = ReportExtensions.GetByline(content, Model);
              var byline= Settings.Content.HighlightKeywords ? ReportExtensions.HighlightKeyWords(section.Value.Filter, rawByline, "byline")  : rawByline;
              var hasPrev = contentCount > 0;
              var prev = hasPrev ? contentCount - 1 : 0;
              var hasNext = (contentCount  + 1) < allContent.Length;
              var next = hasNext ? contentCount + 1 : 0;
              var itemPosition = contentCount;
              contentCount++;
              <div>
                <div id="item-@itemPosition" style="display: flex; align-items: center;">
                  <a name="item-@content.Id"></a>
                </div>
                <div>
                  <a href="#top">top</a>
                  @if (hasPrev )
                  {
                    <a href="#item-@prev">previous</a>
                  }
                  @if (hasNext )
                  {
                    <a href="#item-@next">next</a>
                  }
                </div>
                <h3>@headline</h3>
                <div>@content.Source?.Name</div>
                <div>@content.PublishedOn?.AddHours(utcOffset).ToString("dddd, MMMM d, yyyy")</div>
                @if (!string.IsNullOrWhiteSpace(content.Page))
                {
                    <div>Page @content.Page</div>
                }
                @if (!string.IsNullOrWhiteSpace(byline) && Settings.Headline.ShowByline)
                {
                    <div>By @byline</div>
                }
                <br />
                @if (!String.IsNullOrEmpty(body))
                {
                  <div>@body</div>
                }
                @if (!string.IsNullOrEmpty(content.ImageContent) && section.Value.Settings.ShowImage)
                {
                    var src = $"data:{content.ContentType};base64," + content.ImageContent;
                    <div><img src="@src" alt="@content.FileReferences.FirstOrDefault()?.FileName" /></div>
                }
                @if (Settings.Content.ShowLinkToStory)
                {
                  <a href="@($"{ViewContentUrl}{content.Id}")" target="_blank">View Article</a>
                }
                <hr style="width: 100%; display: inline-block; border-top: 1px solid lightGrey;" />
              </div>
            }
          }
        }
        else if (section.Value.Settings.SectionType == ReportSectionType.TableOfContents)
        {
          @* Report Table of Contents *@
          var tocCount = 0;
          @foreach (var tableSection in Sections.Where(s => s.Value.Settings.SectionType != ReportSectionType.TableOfContents))
          {
            if (!tableSection.Value.Settings.HideEmpty || tableSection.Value.Content.Any())
            {
              <a href="#@($"section-{tableSection.Value.Id}")"><h3 style="margin: 0; padding: 0;">@tableSection.Value.Settings.Label</h3></a>
              @if (section.Value.Settings.ShowHeadlines && tableSection.Value.Content.Any())
              {
                <ul style="margin:0; margin-left: 25px; padding:0;">
                  @foreach (var content in tableSection.Value.Content)
                  {
                    <li>@ReportExtensions.GetFullHeadline(content, Model, utcOffset, true, $"#item-{tocCount}")</li>
                    tocCount++;
                  }
                </ul>
              }
            }
          }
        }

        @* Charts *@
        @if (!section.Value.Settings.ChartsOnTop && section.Value.Settings.ShowCharts)
        {
          foreach (var chart in section.Value.ChartTemplates)
          {
            <img alt="@chart.SectionSettings.AltText" src="@chart.Uid" />
          }
        }
      </div>
    }
  }
}

<p style="font-size: 9pt; margin-top: 0.5rem;">
  Terms of Use - This summary is a service provided by Government Communications and Public Engagement and is only intended for original addressee. All content is the copyrighted property of a third party creator of the material.
  Copying, retransmitting, archiving, redistributing, selling, licensing, or emailing the material to any third party or any employee of the Province who is not authorized to access the material is prohibited.
</p>
'
WHERE "id" = (SELECT CAST("value" AS INTEGER) FROM public."setting" WHERE "name" = 'DefaultReportTemplateId' LIMIT 1);

END $$;
