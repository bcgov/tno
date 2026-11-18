DO $$
BEGIN

UPDATE public."report_section" dst
SET
  "section_type" = CASE
                    WHEN src."settings"->>'sectionType' = 'Content' THEN 0
                    WHEN src."settings"->>'sectionType' = 'TableOfContents' THEN 1
                    WHEN src."settings"->>'sectionType' = 'Text' THEN 2
                    WHEN src."settings"->>'sectionType' = 'MediaAnalytics' THEN 3
                    WHEN src."settings"->>'sectionType' = 'Gallery' THEN 4
                    ELSE 0
                  END
FROM public."report_section" src
WHERE dst."id" = src."id";

END $$;
