DO $$
BEGIN

UPDATE public."report_template" AS a
SET
  "body" = (SELECT
  	REPLACE(REPLACE("body", 'ReportTemplateModel', 'ReportEngineContentModel'), 'Settings.ViewOnWebOnly', 'ViewOnWebOnly')
  	FROM public."report_template"
	WHERE "id" = a."id");

END $$;
