DO $$
DECLARE FolderId INT;
DECLARE ReportId INT;
BEGIN

-- get the 'Event of the Day' Folder id
SELECT Id INTO FolderId
FROM public."folder"
WHERE name = 'Event of the Day'
AND filter_id = (select id from public."filter" where name = 'Event of the Day - Folder Collector')
AND owner_id = (select id from public."user" where username = 'service-account');

-- get the 'Event of the Day' Report id
SELECT Id INTO ReportId
FROM public."report"
WHERE name = 'Event of the Day'
AND owner_id = (select id from public."user" where username = 'service-account');

-- Create invisble report sections with folder that will be cleared on report send.
DELETE public."report_section" 
WHERE "name" = '696adda3-f98a-40d0-8efe-90b614853fde'
AND "report_id" = ReportId,
AND "folder_id" = FolderId;

END $$;
