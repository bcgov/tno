DO $$
BEGIN

DELETE FROM public."report_template_chart_template" where report_template_id = (select id from public."report_template" where "name"  = 'Event of the Day');
DELETE FROM public."report_section" where report_id = (SELECT "id" FROM public."report" WHERE "name" = 'Event of the Day');
DELETE FROM public."report" where "name" = 'Event of the Day';
DELETE FROM public."report_template" where "name" = 'Event of the Day';
DELETE FROM public."folder" where "name" = 'Event of the Day';
DELETE FROM public."filter" where "name" in ('Event of the Day - Folder Collector','Event of the Day - Rolling 365 day aggregate','Event of the Day - 24hr Aggregate');
DELETE FROM public."chart_template" where "name" = 'Topic Analysis';
  
END $$;
