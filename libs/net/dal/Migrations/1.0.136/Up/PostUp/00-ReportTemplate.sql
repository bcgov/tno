DO $$
BEGIN

-- Update custom report with latest template.
UPDATE public."report_template" SET
    "body" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.TemplateEngine.Models.Reports.ReportEngineContentModel>
@using System
@using System.Linq
@using TNO.Entities
@using TNO.TemplateEngine
@{
  var pageBreak = Settings.Sections.UsePageBreaks ? "page-break-after: always;" : "";
  var utcOffset = ReportExtensions.GetUtcOffset(System.DateTime.Now, "Pacific Standard Time");
}

<style>
  a { text-decoration:none; }
</style>

@* LOGO *@
@* original image <center><div style="margin-bottom: 25px; color:#FFFFF6;"><img src="@($"{SubscriberAppUrl}assets/reports/MMI_logo.png")" width="600"></div></center>  *@
@* Temporary path to NEW LOGO (with a white bg) *@
<center><div style="margin-bottom: 25px; color:#FFFFF6;"><img src="https://bobbibjornholt.com/mmi-temp/MMI_logo_report.png" width="600"></div></center>

@* This is the Do Not Forward disclaimer *@
<div><p style="background-color: #FFF7E1; color: #876503; text-align: center; font-size: 1em; font-weight: 700; line-height: 1.1em; letter-spacing: 0.08px; padding: 10px 0px; margin: 6px 0px 20px 0px;">DO NOT FORWARD THIS EMAIL TO ANYONE</p></div>

<a name="top">
  <h1 style="color: #971D29; font-size: 30px; line-height: 34px; margin: 0px; border-bottom: #971D29 1px solid;">@Settings.Subject.Text</h1>
  <div style="font-size:22px; font-weight:500;"><p>@(Settings.Subject.ShowTodaysDate ? $" {ReportExtensions.GetTodaysDate():dd-MMM-yyyy}" : "")</p></div>
</a>

@if (Content.Count() == 0)
{
  <p>There is no content in this report.</p>
}
@if (!ViewOnWebOnly)
{
  var contentCount = 0;
  var allContent = Content.ToArray();
  foreach (var section in Sections)
  {
    var sectionContent = section.Value.Content.ToArray();

    if (!section.Value.IsEnabled) continue;
    if (sectionContent.Length == 0 && section.Value.Settings.HideEmpty) continue;

    <div style="@pageBreak">
      @if (!String.IsNullOrEmpty(section.Value.Settings.Label))
      {
        <div id="section-@section.Value.Id" style="color:#41393B; width:100%; background-color:#F0EEEE; margin: 20px 0px 10px 0px; padding: 4px"> <h2 font-size="16px"><a name="section-@section.Value.Id">@section.Value.Settings.Label</a></h2></div>
      }
      @if (!String.IsNullOrEmpty(section.Value.Description))
      {
        <div style="font-size: 16px; line-height: 110%; margin-bottom: 10px;">@section.Value.Description</div>
      }

      @if (section.Value.SectionType == ReportSectionType.TableOfContents)
      {
        @* TABLE OF CONTENTS SECTION *@
        var tocCount = 0;
        @foreach (var tableSection in Sections.Where(s => new [] {ReportSectionType.Content, ReportSectionType.Gallery}.Contains(s.Value.SectionType)))
        {
          if (!tableSection.Value.Settings.HideEmpty || tableSection.Value.Content.Any())
          {
            <div id="toc">
              <div style="border-bottom:1px solid #675f62; margin: 2px 0 4px 0;">
                <a style="color:#41393B; text-decoration:none;" href="#@($"section-{tableSection.Value.Id}")"><strong>@tableSection.Value.Settings.Label</strong></a>
              </div>

              @if (section.Value.Settings.ShowHeadlines && tableSection.Value.Content.Any() && tableSection.Value.SectionType != ReportSectionType.Gallery)
              {
                <div style="margin: 10px 0 1em 24px; padding:0;">
                  @foreach (var content in tableSection.Value.Content)
                  {
                    var summary = OwnerId.HasValue ? content.Versions.ContainsKey(OwnerId.Value) ? content.Versions[OwnerId.Value].Summary : "" : "";
                    <div style="vertical-align:center; margin-bottom:4px ;"> @ReportExtensions.GetFullHeadline(content, Model, utcOffset, true, $"#item-{tocCount}") @summary</div>
                    tocCount++;
                  }
                </div>
              }
            </div>
          }
        }
      }
      else if (section.Value.SectionType == ReportSectionType.Text)
      {
        @* TEXT SECTION *@
      }
      else if (section.Value.SectionType == ReportSectionType.Content)
      {
        @* STORY CONTENT SECTION *@
        var positionInTOC = 0;
        if (section.Value.Settings.ShowHeadlines)
        {
          if (section.Value.Settings.GroupBy != "")
          {
            var contentGroupings = sectionContent.GetContentGroupings(section.Value.Settings.GroupBy);
            <ul style="margin:0; margin-left: 15px; padding:0;">
              @foreach(KeyValuePair<string, List<long>> contentGroup in contentGroupings)
              {
                <li>@section.Value.Settings.GroupBy = @contentGroup.Key</li>
                <ul style="margin:0; margin-left: 15px; padding:0;">
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
            <ul style="margin:0; margin-left: 15px; padding:0;">
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

        @* FULL STORIES *@
        @if (section.Value.Settings.ShowFullStory)
        {
          for (var i = 0; i < sectionContent.Length; i++)
          {
            var content = sectionContent[i];
            var sentiment = ReportExtensions.GetSentiment(content, Model, true);
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
            <div id="item-@itemPosition" style="display: flex; align-items: center;">
              <a name="item-@content.Id"></a>
            </div>
            <div class="story_separator" style="margin:20px 0 4px 0;" >
              <hr />
            </div>

            @* SECTION & NAVIGATION LINKS *@
            <table width="100%" margin-bottom="25px">
              <tr style="font-size: 12px;">
                <td align="left">
                  <span style="border: none; padding:4px 6px; background-color: #FEE2E5; text-transform: uppercase; font-size:.8rem;"><a name="section-@section.Value.Id" style="text-decoration:none;">@section.Value.Settings.Label</a></span>
                </td>

                <td align="right">
                  <span style="border: 1px solid #ccc; padding:4px 6px; text-transform: uppercase;">
                    <span><a style="text-decoration:none; color: #6750a4; " href="#top">top</a></span>
                    @if (hasPrev )
                    {
                    <span> | <a style="text-decoration:none; color: #6750a4; " href="#item-@prev">previous</a></span>
                    }
                    @if (hasNext )
                    {
                    <span> | <a style="text-decoration:none; color: #6750a4; " href="#item-@next">next</a></span>
                    }
                  </span>
                </td>
              </tr>
            </table>

            <h3 style="margin:16px 0px 10px 0px;">@(String.IsNullOrEmpty(sentiment) ? "" : $"{sentiment} ")@headline</h3>
            <div style="padding: 2px 0px 8px 12px; width:100%; border-bottom:1px solid #E8E9F1; color:#584C50; font-size:.85rem; line-height:.9rem; text-transform:uppercase; margin-bottom:16px;">
              <span style="display: block; font-size: 14px; line-height: 110%;">
                <strong>@content.Source?.Name</strong><br/>
                <strong>@content.PublishedOn?.AddHours(utcOffset).ToString("dddd, MMMM d yyyy")</strong><br/>
                @if (!string.IsNullOrWhiteSpace(content.Page))
                {
                  <span>Page @content.Page</span><br/>
                }
                @if (!string.IsNullOrWhiteSpace(byline) && Settings.Headline.ShowByline)
                {
                  <span>By @byline</span><br/>
                }
              </span>
            </div>
            @if (!String.IsNullOrEmpty(body))
            {
              <div style="font-size:1em; line-height: 110%; margin-bottom: 10px;">@body</div>
            }
            @if (!string.IsNullOrEmpty(content.ImageContent) && section.Value.Settings.ShowImage)
            {
              var src = $"data:{content.ContentType};base64," + content.ImageContent;
              <div><img src="@src" alt="@content.FileReferences.FirstOrDefault()?.FileName" /></div>
            }
            @if (Settings.Content.ShowLinkToStory)
            {
              @* LINK TO STORY ON WEBSITE *@
              <div>
                <span style="font-size: .85em; text-transform:uppercase; vertical-align: middle; background-color:#FFF; border:#ccc 1px solid; margin:20 auto; padding:6px 10px;">
                  <a href="@($"{ViewContentUrl}{content.Id}")" target="_blank" style="color: #6750a4; text-decoration: none;">View Article <img height="14" valign=absmiddle src="https://www.bobbibjornholt.com/mmi-temp/follow_link.png"></a>
                </span>
              </div>
            }
          }
        }
      }
      else if (section.Value.SectionType == ReportSectionType.MediaAnalytics)
      {
        @* Media Analytics Section *@
        foreach (var chart in section.Value.ChartTemplates)
        {
          <img alt="@chart.SectionSettings.AltText" src="@chart.Uid" />
        }
      }
      else if (section.Value.SectionType == ReportSectionType.Gallery)
      {
        @* Gallery Section *@
        var imageDictionary = new Dictionary<string,string>();
        for (var i = 0; i < sectionContent.Length; i++)
        {
          var content = sectionContent[i];
          if (!string.IsNullOrEmpty(content.ImageContent))
          {
            imageDictionary.Add(content.FileReferences.FirstOrDefault()?.FileName,$"data:{content.ContentType};base64," + content.ImageContent);
          }
        }

        if (section.Value.Settings.Direction == "row" && section.Value.Settings.ShowImage)
        {
          <div style="display:flex">
            @foreach(KeyValuePair<string, string> entry in imageDictionary)
            {
              <img style="height:min-content" src="@entry.Value" alt="@entry.Key" />
            }
          </div>
        }
        else if (section.Value.Settings.ShowImage)
        {
          @foreach(KeyValuePair<string, string> entry in imageDictionary)
          {
            <div>
              <img style="height:min-content" src="@entry.Value" alt="@entry.Key" />
            </div>
          }
        }
      }
    </div>
  }
}

@* FOOTER *@
<div style="width:100%">
  <hr style="background-color:#646293; border-width:0;height:1px;line-height:0;width:100%; margin-top:20px; margin-bottom:10px;"/>
  <a style="text-transform:uppercase; color:#fff;" href="@($"{SubscriberAppUrl}{(ReportInstanceId.HasValue ? $"report/instances/{ReportInstanceId }" : $"reports/{ReportId}")}/view")" target="_blank">
    <div style="vertical-align:middle; text-align: center; background-color:#6750A4; border:#ccc 1px solid; margin:20px 20px; padding:6px 10px;">
      View this report as a web page
    </div>
  </a>

  <div style="margin-top:20px; background-color:#F5F6F9; color:#6C5D62; text-align: center; font-size: 11px; font-style: normal; font-weight: 500; line-height: 110%; letter-spacing: 0.08px; padding: 10px 0; width:100%;">
    <p>Terms of Use</p>
    <p>
      This summary is a service provided by Government Communications and Public Engagement and is only intended for original addressee. All content is the copyrighted property of a third party creator of the material.<br /><br />
      <b>Copying, retransmitting, archiving, redistributing, selling, licensing, or emailing the material to any third party or any employee of the Province who is not authorized to access the material is prohibited.</b>
    </p>
  </div>
</div>'
WHERE "name" = 'Custom Report';

END $$;
