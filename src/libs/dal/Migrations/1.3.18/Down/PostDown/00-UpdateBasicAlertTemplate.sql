DO $$
BEGIN

-- Update custom chart with latest template.
UPDATE public."notification_template" SET
    "subject" = '@inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Notification.Models.TemplateModel>
@using System.Linq
@{
  var isAV = @Content.ContentType == TNO.Entities.ContentType.AudioVideo;
  var isTranscriptAvailable = isAV && !string.IsNullOrWhiteSpace(@Content.Body) && @Content.IsApproved;
  var sourceCode = !string.IsNullOrEmpty(@Content.Source?.Code) ? @Content.Source.Code : @Content.OtherSource;
  var body = isTranscriptAvailable ? @Content.Body : (isAV ? @Content.Summary : @Content.Body);
  var transcriptIcon = isTranscriptAvailable ? "▶️📄" : (isAV ? "▶️" : "");
  var toneIcon = "";
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
}@toneIcon@sourceCode: @Content.Headline @transcriptIcon'
WHERE "id" = cast((SELECT value FROM setting
	WHERE name = 'BasicAlertTemplateId') as integer);

END $$;
