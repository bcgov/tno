DO $$
DECLARE ReportId INT;
DECLARE FolderId INT;
DECLARE FilterId INT;
BEGIN

-- get the Report Id
SELECT INTO ReportId id FROM public."report" where "name" = 'Event of the Day' and owner_id = 1;

-- get the Folder Id
SELECT INTO FolderId id FROM public."folder" where "name" = 'Event of the Day' and owner_id = 1;

-- delete section that is no longer required
DELETE FROM public.report_section 
WHERE "settings"->>'label' = 'Hidden Section - DO NOT DELETE - See section summary for logic'
AND report_id = ReportId
AND folder_id = FolderId;

-- get the Filter Id
SELECT INTO FilterId id FROM public."filter" where "name" = 'Event of the Day - Rolling 365 day aggregate' and owner_id = 1;

-- ensure the 365 day section is set correctly
UPDATE public.report_section
SET 
folder_id = null,
filter_id = FilterId
WHERE "settings"->>'label' = 'Top Topics - Last 365 days'
AND report_id = ReportId;

END $$;
