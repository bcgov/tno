DO $$
DECLARE ReportId INT;
DECLARE FilterId INT;
DECLARE FolderId INT;
BEGIN

-- get the Report Id
SELECT INTO ReportId id
FROM public."report"
where "name" = 'Event of the Day'
and owner_id = 1;

-- get the folderId
select INTO FolderId fol.id
from public."folder" fol
join public."filter" fil on fol.filter_id = fil.id
where fol."name" = 'Event of the Day'
and fil."name" = 'Event of the Day - Folder Collector'
and fol.owner_id = 1;

-- get the filterId
select INTO FilterId f.id  
from public."filter" f 
where f."name" = 'Event of the Day - 24hr Aggregate'
and f.owner_id = 1;

-- replace usage of the folder with the filter
-- update the first two sections of the report
update public.report_section
set filter_id = FilterId, folder_id = null
where (sort_order = 0 or sort_order = 1)
and report_id = ReportId;

END $$;
