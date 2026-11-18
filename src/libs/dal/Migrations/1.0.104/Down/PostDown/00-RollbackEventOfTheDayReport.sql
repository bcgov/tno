DO $$
DECLARE ReportId INT;
DECLARE ReportTemplateId INT;
DECLARE ChartTemplateId INT;
DECLARE FolderFilterId INT;
BEGIN
-- get the Report Id
SELECT INTO ReportId id FROM public."report" where "name" = 'Event of the Day' and owner_id = 1;

-- get the Report Template Id
SELECT INTO ReportTemplateId report_template_id FROM public."report" where "name" = 'Event of the Day' and owner_id = 1;

-- get the Chart Template Id
SELECT INTO ChartTemplateId chart_template_id 
FROM public."report_template_chart_template" rtct 
JOIN public."chart_template" ct on rtct."chart_template_id" = ct."id" 
WHERE rtct.report_template_id = ReportTemplateId
AND ct.name = 'Topic Analysis';

-- get the Folder Filter Id
SELECT INTO FolderFilterId filter_id FROM public."folder" where "name" = 'Event of the Day' and owner_id = 1;

-- delete all the newly inserted records related to the Event of the Day report
DELETE FROM public."report_template_chart_template" where report_template_id = ReportTemplateId;
DELETE FROM public."filter" where "id" in (select filter_id from public."report_section" where report_id = ReportId);
DELETE FROM public."report_section" where report_id = ReportId;
DELETE FROM public."report" where id = ReportId;
DELETE FROM public."report_template" where id = ReportTemplateId;
DELETE FROM public."chart_template" where id = ChartTemplateId;
  
-- delete all the newly inserted records related to the Event of the Day folder and filter
DELETE FROM public."folder" where "name" = 'Event of the Day' and owner_id = 1;
DELETE FROM public."filter" where "id" = FolderFilterId;

END $$;
