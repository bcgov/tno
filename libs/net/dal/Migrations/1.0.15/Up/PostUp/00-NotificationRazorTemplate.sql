DO $$
BEGIN

UPDATE public."notification"
SET
	settings=regexp_replace('{ "subject": "@model TNO.Services.Notification.Models.TemplateModel\n
@using System.Linq\n
@{\n
  var isAV = Model.Content.ContentType == TNO.Entities.ContentType.AudioVideo;\n
  var isTranscriptAvailable = isAV && !string.IsNullOrWhiteSpace(Model.Content.Body) && Model.Content.IsApproved;\n
  var sourceCode = !string.IsNullOrEmpty(Model.Content.Source?.Code) ? Model.Content.Source.Code : Model.Content.OtherSource;\n
  var body = isTranscriptAvailable ? Model.Content.Body : (isAV ? Model.Content.Summary : Model.Content.Body);\n

  var transcriptIcon = isTranscriptAvailable ? \"▶️📄\" : (isAV ? \"▶️\" : \"\");\n
  var toneIcon = \"\";\n
  switch (Model.Content.TonePools.FirstOrDefault()?.Value)
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
}

@Raw(toneIcon)@sourceCode: @Model.Content.Headline @Raw(transcriptIcon)
" }', '[\n\r\t]+', '', 'g')::json,
	template='@model TNO.Services.Notification.Models.TemplateModel
@using RazorLight
@{
  var mmiaUrl = Model.NotificationOptions.Value.MmiaUrl?.AbsoluteUri;
  var requestTranscriptUrl = Model.NotificationOptions.Value.RequestTranscriptUrl?.AbsoluteUri;
  var addToReportUrl = Model.NotificationOptions.Value.AddToReportUrl?.AbsoluteUri;
  var isAV = Model.Content.ContentType == TNO.Entities.ContentType.AudioVideo;
  var isTranscriptAvailable = isAV && !string.IsNullOrWhiteSpace(Model.Content.Body) && Model.Content.IsApproved;
  var sourceCode = !string.IsNullOrEmpty(Model.Content.Source?.Code) ? Model.Content.Source.Code : Model.Content.OtherSource;
  var body = isTranscriptAvailable ? Model.Content.Body : (isAV ? Model.Content.Summary : Model.Content.Body);
}
<div>@sourceCode (@Model.Content.Source?.Name)</div>
@if (!string.IsNullOrEmpty(Model.Content.Series?.Name))
{
  <div>@Model.Content.Series.Name</div>
}
<div>@Model.Content.PublishedOn?.ToString("dd-MMM-yyyy hh:mm")</div>
<div>@Raw(body)</div>
<br />
@if (!string.IsNullOrEmpty(mmiaUrl))
{
  <div><a href="@mmiaUrl" target="_blank">MMIA...</a></div>
  <br />
}
@if (isAV && !isTranscriptAvailable && !string.IsNullOrEmpty(requestTranscriptUrl))
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
</div>
'
WHERE name='Basic Alert';

END $$;
