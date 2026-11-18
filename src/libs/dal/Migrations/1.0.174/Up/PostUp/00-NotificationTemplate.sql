DO $$
BEGIN

INSERT INTO public."notification_template" (
  "name"
  , "description"
  , "subject"
  , "body"
  , "is_enabled"
  , "is_public"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'Transcript Approved' -- name
  , 'This email is sent with an approved transcript to subscribers.' -- description
  , 'Transcript Approved' -- subject
  , '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.TemplateEngine.Models.Notifications.NotificationEngineContentModel>
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
  //  body = System.Text.RegularExpressions.Regex.Replace(body, @"\r\n?|\n", "<br/>");
  var replaceWith = "<br/>";
  body = body.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);

  var tz = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
  var utcOffset = tz.GetUtcOffset(System.DateTime.Now).Hours;
}
<div>The transcript has been approved. Please review the content in the body of this email.</div>
<hr/>
<div>@sourceCode (@Content.Source?.Name)</div>
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
</div>' -- body
  , true
  , false
  , 0
  , ''
  , '')
ON CONFLICT("name") DO UPDATE
SET "body" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.TemplateEngine.Models.Notifications.NotificationEngineContentModel>
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
  //  body = System.Text.RegularExpressions.Regex.Replace(body, @"\r\n?|\n", "<br/>");
  var replaceWith = "<br/>";
  body = body.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);

  var tz = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
  var utcOffset = tz.GetUtcOffset(System.DateTime.Now).Hours;
}
<div>The transcript has been approved. Please review the content in the body of this email.</div>
<hr/>
<div>@sourceCode (@Content.Source?.Name)</div>
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
</div>';

END $$;
