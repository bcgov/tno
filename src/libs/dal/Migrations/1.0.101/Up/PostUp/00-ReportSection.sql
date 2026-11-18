DO $$
BEGIN

-- Convert sections with charts to 'MediaAnalytics'
UPDATE public."report_section" AS u
SET
  "settings" = (
    SELECT
  	  "settings" || '{"sectionType": "MediaAnalytics"}'
  	FROM public."report_section"
	  WHERE "id" = u."id")
WHERE "settings" @> '{"showCharts": true}'
  AND ("settings" @> '{"sectionType": "Content"}'
    OR "settings" @> '{"sectionType": "Summary"}');

-- 'Summary' has been changed to 'Text'
UPDATE public."report_section" AS u
SET
  "settings" = (
    SELECT
  	  "settings" || '{"sectionType": "Text"}'
  	FROM public."report_section"
	  WHERE "id" = u."id")
WHERE "settings" @> '{"sectionType": "Summary"}';

END $$;
