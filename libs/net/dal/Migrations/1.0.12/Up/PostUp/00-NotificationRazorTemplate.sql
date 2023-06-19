DO $$
BEGIN

UPDATE public."notification"
SET
	settings=to_json(regexp_replace('{ "subject": "
		@{
			var isTranscriptAvailable = Model.Content.ContentType == ContentType.AudioVideo &&
				!string.IsNullOrWhiteSpace(Model.Content.Body) && Model.Content.IsApproved;

            var transcriptIcon = isTranscriptAvailable ? "check_mark.svg" : "";

            var toneIcon = "face-meh.svg";
            switch (Model.Content.TonePools.FirstOrDefault()?.Value)
            {
                case -3:
                case -4:
                case -5:
                    toneIcon = "face-frown-open.svg";
                    break;
                case 3:
                case 4:
                case 5:
                    toneIcon = "face-grin-wide.svg";
                    break;
            }
		}

		<span>@Model.Content.Source?.Code: @Model.Content.Headline</span>

        @if (!string.IsNullOrEmpty(toneIcon))
        {
            <img src=@toneIcon alt="tone">
        }

        @if (!string.IsNullOrEmpty(transcriptIcon))
        {
            <img src=@transcriptIcon alt="transcript available">
        }
	" }', '\t', '', 'g')),
	template='
        @{
            var mmiaUrl = Model.NotificationOptions.Value.MmiaUrl?.AbsoluteUri;
            var requestTranscriptUrl = Model.NotificationOptions.Value.RequestTranscriptUrl?.AbsoluteUri;
            var addToReportUrl = Model.NotificationOptions.Value.AddToReportUrl?.AbsoluteUri;
        }
        @if (Model.Content.Source != null)
        {
            <div>@Model.Content.Source.Code (@Model.Content.Source.Name)</div>
        }
        @if (!string.IsNullOrEmpty(Model.Content.Series?.Name))
        {
            <div>@Model.Content.Series.Name</div>
        }
        <div>@Model.Content.PublishedOn?.ToString("dd-MMM-yyyy hh:mm")</div>
        <div>@Model.Content.Body</div>
        <br />
        @if (!string.IsNullOrEmpty(mmiaUrl))
        {
            <div><a href="@mmiaUrl" target="_blank">MMIA...</a></div>
            <br />
        }
        @if (Model.Content.ContentType == ContentType.AudioVideo && !string.IsNullOrEmpty(requestTranscriptUrl))
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
        <div style="font-size: small;">
            This e-mail is a service provided by Government Communications and Public Engagement and is only intended
            for the original addressee. All content is the copyrighted property of a third party creator of the material.
            Copying, retransmitting, redistributing, selling, licensing, or emailing the material to any third party or
            any employee of the Province who is not authorized to access the material is prohibited.
        </div>
	'
WHERE name='Basic Alert';

END $$;
