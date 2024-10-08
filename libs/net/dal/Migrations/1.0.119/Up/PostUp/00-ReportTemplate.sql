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
<style>
  a { text-decoration:none; }
  div { font-family:sans-serif; }
</style>
<img src="https://dev.mmi.gov.bc.ca/assets/MMinsights_logo_black.svg" width="600px">

@* This is the Do Not Forward disclaimer *@
<div><p style="background-color: #FFF7E1; color: #876503; text-align: center; font-size: 14px;	font-weight: 500; line-height: 110%; letter-spacing: 0.08px; padding: 10px 0px; margin: 6px 0px 20px 0px;">DO NOT FORWARD THIS EMAIL TO ANYONE</p></div>

<a name="top">
	<h1 style="color: #971D29; font-size: 32px; line-height: 38px; margin: 0px; border-bottom: 1px solid #6C5D62;">@Settings.Subject.Text</h1>
</a>

<div style="font-size:1.12rem; font-weight:600;"><p>@(Settings.Subject.ShowTodaysDate ? $" {ReportExtensions.GetTodaysDate():dd-MMM-yyyy}" : "")</p></div>

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

    <div style="@pageBreak">
      @if (!String.IsNullOrEmpty(section.Value.Settings.Label))
      {
        <h2 id="section-@section.Value.Id" style="margin: 20px 0px 0px 0px; padding: 0;">@section.Value.Settings.Label<a name="section-@section.Value.Id"></a></h2>
      }
      @if (!String.IsNullOrEmpty(section.Value.Description))
      {
        <p>@section.Value.Description</p>
      }

      @if (section.Value.SectionType == ReportSectionType.TableOfContents)
      {
        @* TABLE OF CONTENTS SECTION *@
        var tocCount = 0;
        @foreach (var tableSection in Sections.Where(s => new [] {ReportSectionType.Content, ReportSectionType.Gallery}.Contains(s.Value.SectionType)))
        {
          if (!tableSection.Value.Settings.HideEmpty || tableSection.Value.Content.Any())
          {
            <a style="text-decoration:none;" href="#@($"section-{tableSection.Value.Id}")"><h3 style="border-bottom:1px solid #6C5D62; color:#41393B; font-size:16px; font-weight:800;margin: 10px 0px;">@tableSection.Value.Settings.Label</h3></a>
            @if (section.Value.Settings.ShowHeadlines && tableSection.Value.Content.Any())
            {
              <ul style="margin:0; margin-left: 25px; padding:0;">
                @foreach (var content in tableSection.Value.Content)
                {
                  var summary = OwnerId.HasValue ? content.Versions.ContainsKey(OwnerId.Value) ? content.Versions[OwnerId.Value].Summary : "" : "";
                  <li style="padding-bottom:.5rem;"><div style="font-size:.95rem;">@ReportExtensions.GetFullHeadline(content, Model, utcOffset, true, $"#item-{tocCount}")</div> <div style="font-size:.8rem;">@summary</div></li>
                  tocCount++;
                }
              </ul>
            }
          }
        }
      }
      else if (section.Value.SectionType == ReportSectionType.Text)
      {
        @* TEXT SECTION *@
      }
      else if (section.Value.SectionType == ReportSectionType.Content)
      {
        @* Content Section *@
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

        @* FULL STORIES *@
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
			        <div class="story_separator" style="margin:20px 0 20px 0;" >
                <hr style="background-color:#646293; border-width:0;height:1px;line-height:0;width:100%;"/>
                <div class="section-@section.Value.Id" style="background-color: none; border: none; text-transform: uppercase; font-size:.8rem;">
                  <span style="float:right; border: 1px solid #ccc; border-radius=2px; padding:6px 6px;">
                    <span><a href="#top">top</a></span>
                    @if (hasPrev )
                    {
                      <span> | <a href="#item-@prev">previous</a></span>
                    }
                    @if (hasNext )
                    {
                      <span> | <a href="#item-@next">next</a></span>
                    }
                  </span>
                  <span style="border: none; border-radius: 4px; padding: 2px 6px; margin-left: 0; background-color: #FEE2E5;"><a name="section-@section.Value.Id">@section.Value.Settings.Label</a></span>
                </div>
              </div>
              <h3 style="margin:10px 0px;">@headline</h3>
              <div style="padding: 2px 0px 8px 12px; width:100%; border-bottom:1px solid #E8E9F1; font-size:.85rem; line-height:.9rem; text-transform:uppercase; margin-bottom:16px;">
                <p><strong>@content.Source?.Name</strong></p>
                <p>@content.PublishedOn?.AddHours(utcOffset).ToString("dddd, MMMM d, yyyy")</p>
                @if (!string.IsNullOrWhiteSpace(content.Page))
                {
                  <p>Page @content.Page</p>
                }
                @if (!string.IsNullOrWhiteSpace(byline) && Settings.Headline.ShowByline)
                {
                  <p>By @byline</p>
                }
              </div>
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
                <div style="text-transform:uppercase;"><a style="color: #6750a4;" href="@($"{ViewContentUrl}{content.Id}")" target="_blank">View Article</a></div>
              }
                </div>
              </div>
            </div>
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

        if (section.Value.Settings.Direction == "row")
        {
          <div style="display:flex">
            @foreach(KeyValuePair<string, string> entry in imageDictionary)
            {
              <img style="height:min-content" src="@entry.Value" alt="@entry.Key" />
            }
          </div>
        }
        else
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
<div style="width=100%">
	<hr style="background-color:#646293; border-width:0;height:1px;line-height:0;width:100%; margin-top:20px; margin-bottom:10px;"/>
	<div style="background-color:#6750A4; border:#ccc 1px solid; margin:20px auto; padding:6px 10px; width:fit-content; "><a style="text-transform:uppercase; color:#fff;" href="@SubscriberAppUrl" target="_blank">View this report as a web page</a>
	</div>

	<div style="margin-top:20px; background-color:#F5F6F9; color:#6C5D62; text-align: center; font-size: 11px; font-style: normal; font-weight: 500; line-height: 110%; letter-spacing: 0.08px; padding: 10px 0; width:100%;">
		<p>Terms of Use</p>
		<p> This summary is a service provided by Government Communications and Public Engagement and is only intended for original addressee. All content is the copyrighted property of a third party creator of the material.<br /><br />
		<b>Copying, retransmitting, archiving, redistributing, selling, licensing, or emailing the material to any third party or any employee of the Province who is not authorized to access the material is prohibited.</b>
		</p>
	</div>
</div>'
WHERE "name" = 'Custom Report';

END $$;
