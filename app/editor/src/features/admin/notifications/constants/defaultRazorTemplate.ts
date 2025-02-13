export const defaultRazorTemplate = {
  subject: `@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.TemplateEngine.Models.Notifications.NotificationEngineContentModel>
@using System.Linq
@{
  var isAV = @Content.ContentType == TNO.Entities.ContentType.AudioVideo;
  var isTranscriptAvailable = isAV && !string.IsNullOrWhiteSpace(@Content.Body) && @Content.IsApproved;
  var sourceCode = !string.IsNullOrEmpty(@Content.Source?.Code) ? @Content.Source.Code : @Content.OtherSource;
  var body = isTranscriptAvailable ? @Content.Body : (isAV ? @Content.Summary : @Content.Body);
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
}DEV | @toneIcon@sourceCode: @Content.Headline @transcriptIcon`,
  body: `@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.TemplateEngine.Models.Notifications.NotificationEngineContentModel>
@using System
@{
  var subscriberAppUrl = @SubscriberAppUrl?.ToString();
  var viewContentUrl = @ViewContentUrl?.ToString();
  var requestTranscriptUrl = @RequestTranscriptUrl?.ToString();
  var addToReportUrl = @AddToReportUrl?.ToString();
  var isAV = @Content.ContentType == TNO.Entities.ContentType.AudioVideo;
  var isTranscriptAvailable = isAV && !string.IsNullOrWhiteSpace(@Content.Body) && @Content.IsApproved;
  var sourceCode = !string.IsNullOrEmpty(@Content.Source?.Code) ? @Content.Source.Code : @Content.OtherSource;
  var body = isTranscriptAvailable ? @Content.Body : (isAV ? @Content.Summary : @Content.Body);
  //  body = System.Text.RegularExpressions.Regex.Replace(body, @"\\r\\n?|\\n", "<br/>");
  var replaceWith = "<br/>";
  body = body.Replace("\\r\\n", replaceWith).Replace("\\n", replaceWith).Replace("\\r", replaceWith);

  var tz = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
  var utcOffset = tz.GetUtcOffset(System.DateTime.Now).Hours;
}
<div>@sourceCode (@Content.Source?.Name)</div>
@if (!string.IsNullOrEmpty(@Content.Series?.Name))
{
  <div>@Content.Series.Name</div>
}
<div>@Content.PublishedOn?.AddHours(@utcOffset).ToString("dd-MMM-yyyy hh:mm")</div>
@if (!string.IsNullOrEmpty(@Content.ImageContent))
{
  var src = $"data:{@Content.ContentType};base64," + Content.ImageContent;
  <div><img src="@src" alt="@Content.FileReferences.FirstOrDefault()?.FileName" /></div>
}
<div>@body</div>
<br />
@if (!string.IsNullOrEmpty(subscriberAppUrl))
{
  <div><a href="@subscriberAppUrl" target="">MMI</a> :: <a href="@viewContentUrl" target="_blank">View Article</a></div>
  <br />
}
@if (isAV && !isTranscriptAvailable && !string.IsNullOrEmpty(requestTranscriptUrl) && Content.Source?.DisableTranscribe == false)
{
  <div><a href="@requestTranscriptUrl" target="_blank">Request Transcript...</a></div>
  <br />
}
@if (!string.IsNullOrEmpty(addToReportUrl))
{
  <div><a href="@addToReportUrl" target="_blank">Add to Report</a></div>
  <br />
}
<br />
<div style="font-size: 10px;">
  This e-mail is a service provided by Government Communications and Public Engagement and is only intended
  for the original addressee. All content is the copyrighted property of a third party creator of the material.
  Copying, retransmitting, redistributing, selling, licensing, or emailing the material to any third party or
  any employee of the Province who is not authorized to access the material is prohibited.
</div>`,
};
