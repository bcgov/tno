DO $$
BEGIN

UPDATE public."notification_template"
SET
  "subject" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Notification.Models.TemplateModel>
@{
  var sourceCode = !string.IsNullOrEmpty(@Content.Source?.Name) ? @Content.Source.Name: @Content.OtherSource;
}
Transcript Request Confirmation - @sourceCode: @Content.Headline'
  , "body" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.TemplateEngine.Models.Notifications.NotificationEngineContentModel>
@using System
@{
  var subscriberAppUrl = @SubscriberAppUrl?.ToString();
  var viewContentUrl = @ViewContentUrl?.ToString();
  var requestTranscriptUrl = @RequestTranscriptUrl?.ToString();
  var addToReportUrl = @AddToReportUrl?.ToString();
  var isAV = @Content.ContentType == TNO.Entities.ContentType.AudioVideo;
  var isTranscriptAvailable = isAV && !string.IsNullOrWhiteSpace(@Content.Body) && @Content.IsApproved;
  var sourceCode = !string.IsNullOrEmpty(@Content.Source?.Name) ? @Content.Source.Name: @Content.OtherSource;
  var body = isTranscriptAvailable ? @Content.Body : (isAV ? @Content.Summary : @Content.Body);
  var replaceWith = "<br/>";
  body = body.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);

  var tz = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
  var utcOffset = tz.GetUtcOffset(System.DateTime.Now).Hours;
}
<div>
  <p>Your transcript request has been received.</p>
  <p>When the transcript is completed you will receive another email.</p>
</div>
<hr/>
<div>@sourceCode (@(!String.IsNullOrWhiteSpace(Content.Source?.ShortName) ? Content.Source?.ShortName : Content.Source?.Name))</div>
@if (!string.IsNullOrEmpty(@Content.Series?.Name))
{
  <div>@Content.Series.Name</div>
}
<div>@Content.PublishedOn?.AddHours(@utcOffset).ToString("dd-MMM-yyyy")</div>
@if (!string.IsNullOrEmpty(@Content.Byline))
{
  <div>@Content.Byline</div>
}
<br/>
<div>@body</div>
<br />
@if (!string.IsNullOrEmpty(subscriberAppUrl))
{
  <div><a href="@subscriberAppUrl" target="">MMI</a> :: <a href="@viewContentUrl@Content.Id" target="_blank">View Article</a></div>
  <br />
}
<br />
<div style="font-size: 10px;">
  This e-mail is a service provided by Government Communications and Public Engagement and is only intended
  for the original addressee. All content is the copyrighted property of a third party creator of the material.
  Copying, retransmitting, redistributing, selling, licensing, or emailing the material to any third party or
  any employee of the Province who is not authorized to access the material is prohibited.
</div>'
WHERE "name" = 'Transcript Request Confirmation';

END $$;
