DO $$
BEGIN

INSERT INTO public."report" (
  "name"
  , "description"
  , "owner_id"
  , "report_type"
  , "is_public"
  , "filter"
  , "settings"
  , "template"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'Top Stories Report' -- name
  , 'The top stories report contains a list of top stories.' -- description
  , 1 -- owner_id
  , 1 -- report_type
  , true -- is_public
  , '{
      "size": 1,
      "query": {
        "bool": {
          "must": [
            {
              "range": {
                "publishedOn": {
                  "gte": "now/d"
                }
              }
            },
            {
              "term": {
                "status": "Published"
              }
            },
            {
              "terms": {
                "contentType": [
                  "PrintContent",
                  "Image"
                ]
              }
            },
            {
              "terms": {
                "isHidden": [
                  false
                ]
              }
            },
            {
              "nested": {
                "path": "actions",
                "query": {
                  "term": {
                    "actions.name": "Top Story"
                  }
                }
              }
            }
          ]
        }
      }
	 }'::jsonb -- filter
  , regexp_replace(regexp_replace('{ "subject": "
        @using System.Linq
        @inherits RazorEngineCore.RazorEngineTemplateBase<TNO.TemplateEngine.Models.Reports.TemplateModel>
        @{
            var topStory = Content.FirstOrDefault();
            if (topStory == null) return;
            var byline = string.IsNullOrWhiteSpace(topStory.Byline) ? \"\" : $\"- {topStory.Byline}\";
            var source = string.IsNullOrWhiteSpace(topStory.Source?.Name) ? \"\" : $\"- {topStory.Source?.Name}\";
            var subject = $\"TNO: {topStory.Headline} {byline} {source}\";
        }@subject
    " }', '[\r\t]+', '', 'g'), '[\n]+', '\\n', 'g')::jsonb -- settings
  , '
        @using System.Linq
        @inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.TemplateModel>
        @{
            var topStory = Content.FirstOrDefault();
            if (topStory == null) return;
        }
        <hr />
        <h2 style="margin-bottom: 0.1rem;">@topStory.Headline</h2>
        <div>@topStory.Source?.Name</div>
        <div>@topStory.PublishedOn?.ToString("dd-MMMM-yyyy")</div>
        @if (!string.IsNullOrWhiteSpace(@topStory.Page))
        {
            <div>Page @topStory.Page</div>
        }
        @if (!string.IsNullOrWhiteSpace(@topStory.Byline))
        {
            <div>By @topStory.Byline</div>
        }
        <div>@(!string.IsNullOrWhiteSpace(topStory.Body) ? topStory.Body : topStory.Summary)</div>
        <hr />
        <p style="font-size: 9pt; margin-top: 0.5rem;">
            TERMS OF USE - This e-mail is a service provided by Today''s News Online and is only intended for
            the original addressee. All content is the copyrighted property of a third party creator of the
            material. Copying, retransmitting,redistributing, selling, licensing, or emailing the material
            to any third party or any employee of the Province who is not authorized to access the material is prohibited.
        </p>
    ' -- template
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
);

END $$;
