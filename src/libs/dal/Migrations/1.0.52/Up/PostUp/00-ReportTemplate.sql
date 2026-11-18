DO $$
BEGIN

UPDATE public."report_template" SET
  "body" = REPLACE("body", 'Models.TemplateModel', 'Models.ReportTemplateModel')
WHERE "body" LIKE '%Models.TemplateModel%';

END $$;
