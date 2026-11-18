DO $$
BEGIN

-- Initializes the report template for the custom report.
INSERT INTO public."report_template" (
  "name"
  , "description"
  , "is_enabled"
  , "sort_order"
  , "subject"
  , "body"
  , "settings"
  , "created_by"
  , "updated_by"
) VALUES (
  'Custom Report' -- name
  , 'A customizable report that provides configuration options.' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , '@using TNO.TemplateEngine
@Settings.Subject.Text @(Settings.Subject.ShowTodaysDate ? $" - {ReportExtensions.GetTodaysDate():dd-MMM-yyyy}" : "")' -- subject
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.TemplateEngine.Models.Reports.ReportTemplateModel>
@using System
@using System.Linq
@using TNO.Entities
@using TNO.TemplateEngine
@{
  var pageBreak = Settings.Sections.UsePageBreaks ? "page-break-after: always;" : "";
  var utcOffset = ReportExtensions.GetUtcOffset(System.DateTime.Now, "Pacific Standard Time");
}
<h2 id="top">Report Title</h2>
<div style="color:red;">DO NOT FORWARD THIS EMAIL TO ANYONE</div>
<br/>

@if (Content.Count() == 0)
{
  <p>There is no content in this report.</p>
}
@if (Settings.ViewOnWebOnly)
{
  <a href="">Click to view this report online</>
}
else
{
  foreach (var section in Sections)
  {
    var sectionContent = section.Value.Content.ToArray();
    if (section.Value.IsEnabled && sectionContent.Length > 0 || !Settings.Sections.HideEmpty)
    {
      <div style="@pageBreak">
        @if (!String.IsNullOrEmpty(section.Value.Settings.Label))
          <h3>@section.Value.Settings.Label</h3>
         @if (!String.IsNullOrEmpty(section.Value.Description))
          <p>@section.Value.Description</p>

        @* Charts *@
        @if (section.Value.Settings.ChartsOnTop && section.Value.Settings.ShowCharts)
        {
          foreach (var chart in section.Value.ChartTemplates)
          {
            <img alt="@chart.SectionSettings.AltText" src="@chart.Uid" />
          }
        }

        @* Table of Contents *@
        @if (section.Value.Settings.ShowContent)
        {
          <ul>
            @foreach (var content in sectionContent)
            {
              var headline = $"{(Settings.Headline.ShowSentiment ? content.GetSentimentIcon("{0} - ") : "")}{content.Headline}{(Settings.Headline.ShowShortName && !String.IsNullOrEmpty(content.Source?.ShortName) ? $" - {content.Source?.ShortName}" : (Settings.Headline.ShowSource ? $" - {content.OtherSource}": ""))}{(Settings.Headline.ShowPublishedOn ? $" - {content.PublishedOn?.AddHours(utcOffset):dd-MMM-yyyy}" : "")}";
              if (Settings.Content.IncludeStory)
              {
                <li><a href="#@($"item-{content.Id}")">@headline</a></li>
              }
              else
              {
                <li>@headline</li>
              }
            }
          </ul>

          @* Full Stories *@
          @if (Settings.Content.IncludeStory)
          {
            for (var i = 0; i < sectionContent.Length; i++)
            {
              var content = sectionContent[i];
              var hasPrev = i > 0;
              var prev = hasPrev ? sectionContent[i - 1].Id : 0;
              var hasNext = (i + 1) < sectionContent.Length;
              var next = hasNext ? sectionContent[i + 1].Id : 0;

              <div id="item-@content.Id" style="display: flex; align-items: center;">
                <hr style="width: 100%; display: inline-block;" />
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
              <h3>@content.Headline</h3>
              <div>@content.Source?.Name</div>
              <div>@content.PublishedOn?.AddHours(utcOffset).ToString("dddd, MMMM d, yyyy")</div>
              @if (!string.IsNullOrWhiteSpace(content.Page))
              {
                  <div>Page @content.Page</div>
              }
              @if (!string.IsNullOrWhiteSpace(content.Byline))
              {
                  <div>By @content.Byline</div>
              }
              <br />
              <div>@content.GetBody()</div>
            }
            <hr style="width: 100%; display: inline-block;" />
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
</p>' -- template
  , regexp_replace(regexp_replace('{}', '[\r\t]+', '', 'g'), '[\n]+', '\\n', 'g')::jsonb -- settings
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT DO NOTHING;

END $$;
