DO $$
BEGIN

UPDATE public.report_template SET
    "body" = '
@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.TemplateEngine.Models.Reports.ReportTemplateModel>
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
@if (Settings.ViewOnWebOnly)
{
  <a href="@SubscriberAppUrl" target="_blank">Click to view this report online</a>
}
else
{
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
                       var headline = $"{(Settings.Headline.ShowSentiment ? content.GetSentimentIcon("{0} - ") : "")}{content.Headline}{(Settings.Headline.ShowShortName && !String.IsNullOrEmpty(content.Source?.ShortName) ? $" - {content.Source?.ShortName}" : (Settings.Headline.ShowSource ? $" - {content.OtherSource}": ""))}{(Settings.Headline.ShowPublishedOn ? $" - {content.PublishedOn?.AddHours(utcOffset):dd-MMM-yyyy}" : "")}";
                       if (section.Value.Settings.ShowFullStory)
                       {
                         <li><a href="#item-@content.Id">@headline</a></li>
                       }
                       else
                       {
                         <li><a href="@($"{@ViewContentUrl}{content.Id}")" target="_blank">@headline</a></li>
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
                   var headline = $"{(Settings.Headline.ShowSentiment ? content.GetSentimentIcon("{0} - ") : "")}{content.Headline}{(Settings.Headline.ShowShortName && !String.IsNullOrEmpty(content.Source?.ShortName) ? $" - {content.Source?.ShortName}" : (Settings.Headline.ShowSource ? $" - {content.OtherSource}": ""))}{(Settings.Headline.ShowPublishedOn ? $" - {content.PublishedOn?.AddHours(utcOffset):dd-MMM-yyyy}" : "")}";
                    @if (section.Value.Settings.ShowFullStory)
                    {
                      <li><a href="#item-@content.Id">@headline</a></li>
                    }
                    else
                    {
                      <li><a href="@($"{@ViewContentUrl}{content.Id}")" target="_blank">@headline</a></li>
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
              var headline= Settings.Content.HighlightKeywords ? section.Value.Filter.HighlightKeyWords(content.Headline, "headline")  : content.Headline;
              var body = Settings.Content.HighlightKeywords ? section.Value.Filter.HighlightKeyWords(content.GetBody(), "body")  : content.GetBody();
              var byline= Settings.Content.HighlightKeywords ? section.Value.Filter.HighlightKeyWords(content.Byline, "byline")  : content.Byline;
              var hasPrev = i > 0;
              var prev = hasPrev ? sectionContent[i - 1].Id : 0;
              var hasNext = (i + 1) < sectionContent.Length;
              var next = hasNext ? sectionContent[i + 1].Id : 0;
              <div>
                <div id="item-@content.Id" style="display: flex; align-items: center;">
                  <a name="item-@content.Id"></a>
                </div>
                <div>
                  <a href="#top">top</a>
                  @if (i > 0)
                  {
                    <a href="#item-@prev">previous</a>
                  }
                  @if (i < sectionContent.Length -1)
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
                    var headline = $"{(Settings.Headline.ShowSentiment ? content.GetSentimentIcon("{0} - ") : "")}{content.Headline}{(Settings.Headline.ShowShortName && !String.IsNullOrEmpty(content.Source?.ShortName) ? $" - {content.Source?.ShortName}" : (Settings.Headline.ShowSource ? $" - {content.OtherSource}": ""))}{(Settings.Headline.ShowPublishedOn ? $" - {content.PublishedOn?.AddHours(utcOffset):dd-MMM-yyyy}" : "")}";
                    <li><a href="#item-@content.Id">@headline</a></li>
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
WHERE "name" = 'Custom Report';

END $$;
