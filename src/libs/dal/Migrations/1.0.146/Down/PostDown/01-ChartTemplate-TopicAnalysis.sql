DO $$
DECLARE ServiceAccountId INT;
DECLARE ReportId INT;
DECLARE ReportTemplateId INT;
DECLARE FolderFilterId INT;
DECLARE ReportSectionId INT;
DECLARE ChartTemplateId INT;
BEGIN

SELECT id INTO ServiceAccountId
FROM public."user" where username = 'service-account';

-- get the Report Template Id
SELECT INTO ReportTemplateId report_template_id FROM public."report" where "name" = 'Event of the Day' and owner_id = 1;

-- get the Chart Template Id
SELECT INTO ChartTemplateId chart_template_id 
FROM public."report_template_chart_template" rtct 
JOIN public."chart_template" ct on rtct."chart_template_id" = ct."id" 
WHERE rtct.report_template_id = ReportTemplateId
AND ct.name = 'Topic Analysis';

-- Revert the 'Topic Analysis' chart template name change
UPDATE public."chart_template"
SET "name" = 'Topic Analysis'
WHERE id = ChartTemplateId;

END $$;
