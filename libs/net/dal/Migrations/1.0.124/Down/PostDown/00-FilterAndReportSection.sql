DO $$
DECLARE ServiceAccountId INT;
DECLARE ChartTemplateId INT;
DECLARE ReportId INT;
DECLARE ReportTemplateId INT;
DECLARE FilterId INT;
BEGIN

SELECT id INTO ServiceAccountId
FROM public."user" where username = 'service-account';

SELECT Id INTO ChartTemplateId
FROM public."chart_template"
WHERE name = 'Topic Type - last 28 days';

SELECT Id INTO FilterId
FROM public."filter"
WHERE name = 'Event of the Day - Topic Type - last 28 days' -- name
AND owner_id = ServiceAccountId;

-- get the 'Event of the Day' Report id and associated Report Template id
SELECT id, report_template_id INTO ReportId, ReportTemplateId
FROM public."report"
WHERE name = 'Event of the Day'
AND owner_id = ServiceAccountId;

DELETE FROM public."report_template_chart_template" 
WHERE "report_template_id" = ReportTemplateId
AND "chart_template_id" = ChartTemplateId;

-- delete report section - cascading delete clears up other associated data.
DELETE FROM public."report_section" 
WHERE "name" = '395f28be-4b7f-491b-a788-f1de86d09b7c' -- name'
AND "report_id" = ReportId
AND "filter_id" = FilterId;

DELETE FROM public."chart_template" 
WHERE "id" = ChartTemplateId;

DELETE FROM public."filter" 
WHERE "id" = FilterId;

END $$;
