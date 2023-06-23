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
  'Frontpages Report' -- name
  , 'The frontpages report contains a list of Front Page images.' -- description
  , 1 -- owner_id
  , 1 -- report_type
  , true -- is_public
  , '{
      "size": 500,
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
              "bool": {
                "must": [
                  { "match": { "product.name": "Front Page" } },
                  {
                    "terms": {
                      "sourceId": [
                        91,
                        246,
                        247,
                        248,
                        249
                      ]
                    }
                  }
                ]
              }
            }
          ]
        }
      }
	 }'::jsonb -- filter
  , '{ "subject": "TNO: Front Pages" }'::jsonb -- settings
  , '
        @using TNO.Entities
        @inherits RazorEngineCore.RazorEngineTemplateBase<TNO.Services.Reporting.Models.TemplateModel>

        @{
            var frontPages = Content.Where(x => x.ContentType == ContentType.Image);
        }

        @foreach (var frontPage in frontPages)
        {
            if (!string.IsNullOrEmpty(frontPage.ImageContent))
            {
                var src = $"data:{frontPage.ContentType};base64," + frontPage.ImageContent;
                <div>
                    <img src="@src" alt="@frontPage.FileReferences.FirstOrDefault()?.FileName" />
                </div>
            }
        }

        <p style="font-size: 9pt; margin-top: 0.5rem;">
            TERMS OF USE - This e-mail is a service provided by Today''s News Online and is only intended for
            the originaladdressee. All content is the copyrighted property of a third party creator of the
            material. Copying, retransmitting,redistributing, selling, licensing, or emailing the material
            to any third party or any employee of the Province who is notauthorized to access the material is prohibited.
        </p>
    ' -- template
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
);

END $$;
