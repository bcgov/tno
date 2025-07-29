export const defaultRazorTemplate = {
  subject: `@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.TemplateEngine.Models.Notifications.NotificationEngineContentModel>
@using System.Linq
@{
  var isAV = @Content.ContentType == TNO.Entities.ContentType.AudioVideo;
  var useSummary = @Content.ContentType == TNO.Entities.ContentType.Image || isAV;
  var isTranscriptAvailable = isAV && !string.IsNullOrWhiteSpace(@Content.Body) && @Content.IsApproved;
  var sourceCode = !string.IsNullOrEmpty(@Content.Source?.Code) ? @Content.Source.Code : @Content.OtherSource;
  var body = isTranscriptAvailable ? @Content.Body : (useSummary ? @Content.Summary : @Content.Body);
  var transcriptIcon = isTranscriptAvailable ? "▶️📄" : (isAV ? "▶️" : "");
  var toneIcon = "";
  if (@EnableReportSentiment)
  {
    switch (@Content.TonePools.FirstOrDefault()?.Value)
    {
      case 0:
        toneIcon = "😐 ";
        break;
      case -3:
      case -4:
      case -5:
        toneIcon = "☹️ ";
        break;
      case 3:
      case 4:
      case 5:
        toneIcon = "🙂 ";
        break;
    }
  }
}@toneIcon@sourceCode: @Content.Headline @transcriptIcon`,
  body: `@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.TemplateEngine.Models.Notifications.NotificationEngineContentModel>
@using System
@using System.Web
@using System.Linq
@{
  var subscriberAppUrl = @SubscriberAppUrl?.ToString();
  var viewContentUrl = @ViewContentUrl?.ToString();
  var requestTranscriptUrl = @RequestTranscriptUrl?.ToString();
  var addToReportUrl = @AddToReportUrl?.ToString();
  var isAV = @Content.ContentType == TNO.Entities.ContentType.AudioVideo;
  var useSummary = @Content.ContentType == TNO.Entities.ContentType.Image || isAV;
  var isTranscriptAvailable = isAV && !string.IsNullOrWhiteSpace(@Content.Body) && @Content.IsApproved;
  var sourceCode = !string.IsNullOrEmpty(@Content.Source?.Code) ? @Content.Source.Code : @Content.OtherSource;
  var body = isTranscriptAvailable ? @Content.Body : (useSummary ? @Content.Summary : @Content.Body);
  var replaceWith = "<br/>";
  body = body.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);
  var filePath = @Content.FileReferences.FirstOrDefault()?.Path;

  var tz = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
  var utcOffset = tz.GetUtcOffset(System.DateTime.Now).Hours;
}
@if (!string.IsNullOrWhiteSpace(@Content.Source?.Name))
{
  <div>@Content.Source.Name</div>
}
@if (!string.IsNullOrEmpty(@Content.Series?.Name))
{
  <div>@Content.Series.Name</div>
}
<div>@Content.PublishedOn?.AddHours(@utcOffset).ToString("dd-MMM-yyyy HH:mm")</div>
@if (!string.IsNullOrEmpty(@Content.Byline))
{
  <div>@Content.Byline</div>
}
@if (!string.IsNullOrEmpty(@Content.Page))
{
  <div>Page #@Content.Page</div>
}
@if (@Content.ContentType == TNO.Entities.ContentType.Image && !string.IsNullOrEmpty(filePath))
{
  var apiFileUrl = subscriberAppUrl + "api/subscriber/contents/download?path=" + filePath;
  <div><img src="@apiFileUrl" alt="@Content.FileReferences.FirstOrDefault()?.FileName" /></div>
}
<br/>
<p>
<hr></hr>
<div>@body</div>
<br />
@if (!string.IsNullOrEmpty(subscriberAppUrl))
{
  <div><a href="@subscriberAppUrl" target="">MMI</a> : <a href="@viewContentUrl@Content.Id" target="_blank">View article on MMI website</a></div>
  <br />
}
@if (isAV && !isTranscriptAvailable && !string.IsNullOrEmpty(requestTranscriptUrl) && Content.Source?.DisableTranscribe == false)
{
  <div><a href="@($"{requestTranscriptUrl}{Content.Id}")?uid=@(Content.OwnerId)&headline=@(HttpUtility.UrlEncode(Content.Headline))&source=@(HttpUtility.UrlEncode(Content.OtherSource))" target="_blank"
                 style="margin-left:auto; padding:0.5em; color:#123456; background-color:#fff7e1; border:2px solid #dcc797; height:2em; border-radius:0.85em; text-decoration:none; display:inline-flex; align-items:center; gap:0.5em;"
                 onmouseover="this.style.backgroundColor='#ffebb3';"
                 onmouseout="this.style.backgroundColor='#fff7e1';">
                   <i class="fa fa-feather" style="color:#cccccc;"></i> Request Transcript</a>
                   </div>
  <br />
}
<br /><hr></hr>
<div style="font-size: 14px;">
<b>This e-mail is a service provided by Government Communications and Public Engagement and is only intended
  for the original addressee. All content is the copyrighted property of a third party creator of the material. Copying, re-transmitting, redistributing, selling, licensing, or e-mailing the material to any third party or any employee of the Province who is not authorized to access the material is prohibited. </b>
</div>`,
};
