DO $$
BEGIN

-- Convert sections with charts back to 'Content'
UPDATE public."report_section" AS u
SET
  "settings" = (
    SELECT
  	  "settings" || '{"sectionType": "Content", "showCharts": true}'
  	FROM public."report_section"
	  WHERE "id" = u."id")
WHERE "settings" @> '{"sectionType": "MediaAnalytics"}';

-- 'Text' has been changed to 'Summary'
UPDATE public."report_section" AS u
SET
  "settings" = (
    SELECT
  	  "settings" || '{"sectionType": "Summary"}'
  	FROM public."report_section"
	  WHERE "id" = u."id")
WHERE "settings" @> '{"sectionType": "Text"}';

END $$;
