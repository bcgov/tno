DO $$
BEGIN

UPDATE public.report_template SET
  "body" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ReportEngineAVOverviewModel>
@using System
@using System.Linq
@using TNO.Entities

<style>
a { text-decoration:none;
{
</style> 
<div style="padding-bottom:20px;"><img src="https://dev.mmi.gov.bc.ca/assets/MMinsights_logo_black.svg" width="500px"></div>

<h1 id="top" style="color: #971D29; font-size: 32px; line-height: 38px; margin: 0px; border-bottom: 1px solid #6C5D62;">Evening Overview</h1>

<div style="font-size:1.12rem; font-weight:600;"><p>@($"{Instance.PublishedOn:dddd, MMMM dd, yyyy}")</p></div>

<br/>
@if (Instance.Sections.Count() == 0)
{
  <p>There is no content in this report.</p>
}
@foreach (var section in Instance.Sections)
{
  var itemIndex = 0;
  var itemCount = section.Items.Count(i => i.ItemType == AVOverviewItemType.Story);
  
  <div class="section" style="border: 1px solid #9F9196; border-radius: 6px; margin-bottom:20px; padding: 16px;width:1150px;">
  
	<div class="section-header" style="text-align:center; font-size:1.1rem; font-weight:600; padding-bottom:6px;">
      @section.Name @(section.StartTime?.Length == 8 ? section.StartTime.Substring(0, 5) : section.StartTime) @(section.StartTime?.Length > 0 ? "hr" : "")
      @if (!String.IsNullOrEmpty(section.Anchors)) { <span> - @section.Anchors</span> }
    </div>
	
	
    <table style="width:100%; border: collapse;">
      <thead style="background-color:#9F9196;">
        <tr>
          <td style="text-transform:uppercase; font-weight:500; color:#FFF; padding:0 10px; width:8rem;">Placement</td>
          <td style="text-transform:uppercase; font-weight:500; color:#FFF; padding:0 10px; width:6rem;">Time</td>
          <td style="text-transform:uppercase; font-weight:500; color:#FFF; padding:0 10px;">Story</td>
        </tr>
      </thead>
	  
	  
      @foreach (var item in section.Items)
      {
        var placement = item.ItemType switch
        {
          AVOverviewItemType.Intro => "Intro",
          AVOverviewItemType.Story => itemIndex  == 1 ? "Top Story" : $"{itemIndex}/{itemCount}",
          AVOverviewItemType.Ad => "",
          _ => $"{item.ItemType}",
        };
        var time = item.ItemType switch
        {
          AVOverviewItemType.Ad => "",
          _ => $"{item.Time}",
        };
        var summary = String.IsNullOrEmpty(item.Summary) ? "---" : item.Summary.Replace("\n", "<br/>");
        <tr style="border-bottom: 1px solid #DED9DA;">
          <td style="border-bottom: 1px solid #DED9DA; font-size:.9rem; padding:2px 2px 2px 10px;">@placement</td>
          <td style="border-bottom: 1px solid #DED9DA; font-size:.9rem; padding:2px 2px 2px 10px;">@(time?.Length == 8 ? time.Substring(0, 5) : time)</td>
          <td style="border-bottom: 1px solid #DED9DA; font-size:.9rem; padding:2px 2px 2px 10px;">
            @if (item.ContentId.HasValue)
            {
              <a href="@($"{ViewContentUrl}{item.ContentId}")">@summary</a>
              @if (item.Content.FileReferences.Count() > 0 && item.Content.FileReferences.First().ContentType.StartsWith("audio/"))
              {
                <a class="icon video-icon" href="@($"{ViewContentUrl}{item.ContentId}")">
                  <svg class="video-icon" xmlns="http://www.w3.org/2000/svg" height="16" width="18" viewBox="0 0 576 512"><!--!Font Awesome Free 6.5.1 by fontawesome - https://fontawesome.com License - https://fontawesome.com/license/free Copyright 2023 Fonticons, Inc.--><path d="M0 128C0 92.7 28.7 64 64 64H320c35.3 0 64 28.7 64 64V384c0 35.3-28.7 64-64 64H64c-35.3 0-64-28.7-64-64V128zM559.1 99.8c10.4 5.6 16.9 16.4 16.9 28.2V384c0 11.8-6.5 22.6-16.9 28.2s-23 5-32.9-1.6l-96-64L416 337.1V320 192 174.9l14.2-9.5 96-64c9.8-6.5 22.4-7.2 32.9-1.6z"/></svg>
                </a>
              }
              @if (item.Content.ContentType.ToString() == "AudioVideo" && item.Content.Summary.Length > 0 && item.Content.IsApproved)
              {
                <a class="icon" href="@($"{ViewContentUrl}{item.ContentId}")">
                  <svg class="transcript-icon" xmlns="http://www.w3.org/2000/svg" height="16" width="18" viewBox="0 0 576 512"><!--!Font Awesome Free 6.5.1 by fontawesome - https://fontawesome.com License - https://fontawesome.com/license/free Copyright 2023 Fonticons, Inc.--><path d="M0 80v48c0 17.7 14.3 32 32 32H48 96V80c0-26.5-21.5-48-48-48S0 53.5 0 80zM112 32c10 13.4 16 30 16 48V384c0 35.3 28.7 64 64 64s64-28.7 64-64v-5.3c0-32.4 26.3-58.7 58.7-58.7H480V128c0-53-43-96-96-96H112zM464 480c61.9 0 112-50.1 112-112c0-8.8-7.2-16-16-16H314.7c-14.7 0-26.7 11.9-26.7 26.7V384c0 53-43 96-96 96H368h96z"/></svg>
                  </a>
              }
            }
            else
            {
              @summary
            }
          </td>
        </tr>
        @{
          itemIndex++;
        }
      }
    </table>
  </div>
}
@* FOOTER *@

	<div style="margin-top:20px; background-color:#F5F6F9; color:#6C5D62; text-align: center; font-size: 11px; font-style: normal; font-weight: 500; line-height: 110%; letter-spacing: 0.08px; padding: 10px 0; width:100%;">
		<p>Terms of Use</p>
		<p> This summary is a service provided by Government Communications and Public Engagement and is only intended for original addressee. All content is the copyrighted property of a third party creator of the material.<br /><br />
		<b>Copying, retransmitting, archiving, redistributing, selling, licensing, or emailing the material to any third party or any employee of the Province who is not authorized to access the material is prohibited.</b>
		</p>
	</div>
</div>'
WHERE "name" = 'AV Overview - Weekday';

END $$;
