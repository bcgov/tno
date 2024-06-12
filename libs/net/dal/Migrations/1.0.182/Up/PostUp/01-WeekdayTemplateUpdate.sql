DO $$
BEGIN

UPDATE public.report_template SET
"body" = '
@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.ReportEngineAVOverviewModel>
@using System
@using System.Linq
@using TNO.Entities
<style>
a { text-decoration:none; }
.video-icon { fill:#702828; height:12px; width:12px; margin:1.5px; }
.transcript-icon { fill:#047A8C; height:12px; width:12px; margin:1.5px; }
.icon { float: right; }
.main-body { width: 595px; }
.main-header { width:593px; background-color:#FFF7E1; color:#876503; text-align:center; font-size:16px;	font-weight:700; line-height:110%; letter-spacing:0.08px; padding:10px 0px; margin:6px 0px 20px 0px; }
.column-header { text-transform:uppercase; font-weight:500; color:#FFF; padding:0 2px; font-size:12px; }
.column-data { border-bottom:1px solid #DED9DA; font-weight:500; font-size:11px; padding:2px; }
.story-content { white-space:nowrap; text-overflow:ellipsis; overflow:hidden; max-width:438px; display:inline-block; }
.footer-content { background-color:#F5F6F9; color:#6C5D62; text-align:center; font-size:11px; font-style:normal; font-weight:500; line-height:110%; letter-spacing:0.08px; word-wrap: break-word; margin:0px; padding:0px; }
.footer-table { border:none; width:595px; border-collapse:collapse; empty-cells:show; border:collapse; }
</style>

<table cellspacing="1" cellpadding="1" class="main-body">
<tr><td>
  <div><p class="main-header">This TNO product is intended only for the use of the person to whom it is addressed. Please do not forward or redistribute.</p></div>
  <table cellspacing="1" cellpadding="1" class="main-body">
  <tr><td style="padding-top: 10px; border-radius:25px; background-color:rgba(255,255,255, .25)">
    <div><img src="https://dev.mmi.gov.bc.ca/assets/MMinsights_logo_black.svg" width="500px"></div>
	</td></tr>
    <tr style"height:20px;"><td>&nbsp;</td></tr>
	<tr><td>
    <h1 id="top" style="color: #971D29; font-size: 32px; line-height: 38px; margin: 0px; border-bottom: 1px solid #6C5D62;">Evening Overview</h1>

    <div style="font-size:16px; font-weight:600;"><p>@($"{Instance.PublishedOn:dddd, MMMM dd, yyyy}")</p></div>

	</td></tr>
	<tr><td>
@if (Instance.Sections.Count() == 0)
{
  <p>There is no content in this report.</p>
}
@foreach (var section in Instance.Sections)
{
  var itemIndex = 0;
  var itemCount = section.Items.Count(i => i.ItemType == AVOverviewItemType.Story);
  <br/>
  <table class="section" style="border: 1px solid #9F9196; border-radius:6px; margin:0px; padding:0px; width:591px; empty-cells:show;">
    <tr style"height:16px;"><td>&nbsp;</td></tr>
	<tr><td class="section-header" style="text-align:center; font-size:18px; font-weight:600; padding-bottom:6px;">
      @section.Name @(section.StartTime?.Length == 8 ? section.StartTime.Substring(0, 5) : section.StartTime) @(section.StartTime?.Length > 0 ? "hr" : "")
      @if (!String.IsNullOrEmpty(section.Anchors)) { <span> - @section.Anchors</span> }
    </td></tr>
    <tr><td style="border:0px; padding:0px;">
    <table style="width:100%; border: collapse;">
      <thead style="background-color:#9F9196;">
        <tr>
          <td align ="center" class="column-header" style="width:64px;">Placement</td>
          <td align ="center" class="column-header" style="width:28.5px;">Time</td>
          <td align ="center" class="column-header">Story</td>
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
          <td align ="center" class="column-data">@placement</td>
          <td align ="center" class="column-data">@(time?.Length == 8 ? time.Substring(0, 5) : time)</td>
          <td align ="left" class="column-data" style="max-width: 480px;">
			@if (item.ContentId.HasValue)
            {
              <a href="@($"{ViewContentUrl}{item.ContentId}")" class="story-content">@summary</a>
              @if (item.Content.FileReferences != null && item.Content.FileReferences.Any() && item.Content.FileReferences.FirstOrDefault().ContentType.StartsWith("video/") )
              {
                <a class="icon" href="@($"{ViewContentUrl}{item.ContentId}")">
<svg class="video-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 576 512"><path d="M0 128C0 92.7 28.7 64 64 64H320c35.3 0 64 28.7 64 64V384c0 35.3-28.7 64-64 64H64c-35.3 0-64-28.7-64-64V128zM559.1 99.8c10.4 5.6 16.9 16.4 16.9 28.2V384c0 11.8-6.5 22.6-16.9 28.2s-23 5-32.9-1.6l-96-64L416 337.1V320 192 174.9l14.2-9.5 96-64c9.8-6.5 22.4-7.2 32.9-1.6z"/></svg>
                </a>
              }
              @if (item.Content.ContentType.ToString() == "AudioVideo" && item.Content.Summary.Length > 0 && item.Content.IsApproved)
              {
                <a class="icon" href="@($"{ViewContentUrl}{item.ContentId}")">
<svg style="float: right" class="transcript-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"><path d="M278.5 215.6L23 471c-9.4 9.4-9.4 24.6 0 33.9s24.6 9.4 33.9 0l57-57h68c49.7 0 97.9-14.4 139-41c11.1-7.2 5.5-23-7.8-23c-5.1 0-9.2-4.1-9.2-9.2c0-4.1 2.7-7.6 6.5-8.8l81-24.3c2.5-.8 4.8-2.1 6.7-4l22.4-22.4c10.1-10.1 2.9-27.3-11.3-27.3l-32.2 0c-5.1 0-9.2-4.1-9.2-9.2c0-4.1 2.7-7.6 6.5-8.8l112-33.6c4-1.2 7.4-3.9 9.3-7.7C506.4 207.6 512 184.1 512 160c0-41-16.3-80.3-45.3-109.3l-5.5-5.5C432.3 16.3 393 0 352 0s-80.3 16.3-109.3 45.3L139 149C91 197 64 262.1 64 330v55.3L253.6 195.8c6.2-6.2 16.4-6.2 22.6 0c5.4 5.4 6.1 13.6 2.2 19.8z"/></svg>
                  </a>
              }
            }
            else
            {
              <span class="story-content">@summary</span>
            }
          </td>
        </tr>
        @{
          itemIndex++;
        }
      }
	</table>
  </td></tr>
  <tr style"height:16px;"><td>&nbsp;</td></tr>
  </table>
}

  </td></tr>
  <tr style"height:16px;"><td>&nbsp;</td></tr>
  <tr><td>
@* FOOTER *@
    <table cellspacing="0" cellpadding="0" class="footer-table">
      <tr style"height:10px;"><td class="footer-content">&nbsp;</td></tr>
	  <tr><td class="footer-content">
	    <p>Terms of Use</p>
	  </td></tr>
	  <tr><td class="footer-content">
	    This summary is a service provided by Government Communications and Public Engagement and is only intended for original addressee. All content is the copyrighted property of a third party creator of the material.
	  </td></tr>
	  <tr style"height:10px;"><td class="footer-content">&nbsp;</td></tr>
	  <tr><td class="footer-content">
	    <b>Copying, retransmitting, archiving, redistributing, selling, licensing, or emailing the material to any third party or any employee of the Province who is not authorized to access the material is prohibited.</b>
	  </td></tr>
	  <tr style"height:10px;"><td class="footer-content">&nbsp;</td></tr>
   </table>

  </td></tr>
  </table>
</td></tr>
</table>
'

WHERE "name" = 'Evening Overview - Weekday';

END $$;
