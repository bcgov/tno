DO $$
BEGIN

UPDATE public."notification" SET
  "settings" = regexp_replace(regexp_replace('{ "subject": "@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Notification.Models.TemplateModel>
@using System.Linq
@{
  var isAV = @Content.ContentType == TNO.Entities.ContentType.AudioVideo;
  var isTranscriptAvailable = isAV && !string.IsNullOrWhiteSpace(@Content.Body) && @Content.IsApproved;
  var sourceCode = !string.IsNullOrEmpty(@Content.Source?.Code) ? @Content.Source.Code : @Content.OtherSource;
  var body = isTranscriptAvailable ? @Content.Body : (isAV ? @Content.Summary : @Content.Body);
  var transcriptIcon = isTranscriptAvailable ? \"▶️📄\" : (isAV ? \"▶️\" : \"\");
  var toneIcon = \"\";
  switch (@Content.TonePools.FirstOrDefault()?.Value)
  {
    case 0:
      toneIcon = \"😐 \";
      break;
    case -3:
    case -4:
    case -5:
      toneIcon = \"☹️ \";
      break;
    case 3:
    case 4:
    case 5:
      toneIcon = \"🙂 \";
      break;
  }
}@toneIcon@sourceCode: @Content.Headline @transcriptIcon" }', '[\r\t]+', '', 'g'), '[\n]+', '\\n', 'g')::jsonb
  , "template" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Notification.Models.TemplateModel>
@{
  var mmiaUrl = @MmiaUrl?.AbsoluteUri;
  var viewContentUrl = @ViewContentUrl?.AbsoluteUri;
  var requestTranscriptUrl = @RequestTranscriptUrl?.AbsoluteUri;
  var addToReportUrl = @AddToReportUrl?.AbsoluteUri;
  var isAV = @Content.ContentType == TNO.Entities.ContentType.AudioVideo;
  var isTranscriptAvailable = isAV && !string.IsNullOrWhiteSpace(@Content.Body) && @Content.IsApproved;
  var sourceCode = !string.IsNullOrEmpty(@Content.Source?.Code) ? @Content.Source.Code : @Content.OtherSource;
  var body = isTranscriptAvailable ? @Content.Body : (isAV ? @Content.Summary : @Content.Body);
}
<div>@sourceCode (@Content.Source?.Name)</div>
@if (!string.IsNullOrEmpty(@Content.Series?.Name))
{
  <div>@Content.Series.Name</div>
}
<div>@Content.PublishedOn?.ToString("dd-MMM-yyyy hh:mm")</div>
<div>@body</div>
<br />
@if (!string.IsNullOrEmpty(mmiaUrl))
{
  <div><a href="@mmiaUrl" target="_blank">MMIA</a> :: <a href="@viewContentUrl" target="_blank">View Article</a></div>
  <br />
}
@if (isAV && !isTranscriptAvailable && !string.IsNullOrEmpty(requestTranscriptUrl))
{
  <div><a href="@requestTranscriptUrl" target="_blank">Request Transcript</a></div>
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
</div>
'
WHERE "name" = 'Basic Alert';

END $$;
