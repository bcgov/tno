DO $$
BEGIN

UPDATE public."report_template" SET
  "body" = REPLACE("body", 'Models.ReportTemplateModel', 'Models.TemplateModel')
WHERE "body" LIKE '%Models.ReportTemplateModel%';

END $$;
