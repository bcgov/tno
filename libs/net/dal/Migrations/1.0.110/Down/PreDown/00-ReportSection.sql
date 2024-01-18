DO $$
BEGIN

UPDATE public."report_section" dst
SET
  "settings" = jsonb_set(src."settings", '{sectionType}', to_jsonb(CASE
                    WHEN src."section_type" = 0 THEN 'Content'
                    WHEN src."section_type" = 1 THEN 'TableOfContents'
                    WHEN src."section_type" = 2 THEN 'Text'
                    WHEN src."section_type" = 3 THEN 'MediaAnalytics'
                    WHEN src."section_type" = 4 THEN 'Gallery'
                    ELSE 'Content'
                  END))
FROM public."report_section" src
WHERE dst."id" = src."id";

END $$;
