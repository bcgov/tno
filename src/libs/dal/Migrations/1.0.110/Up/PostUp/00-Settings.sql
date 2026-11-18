INSERT INTO public."setting" (
  "name"
  , "description"
  , "value"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
)
VALUES (
  'DefaultReportTemplateId'
  , 'The default report template for subscriber reports.'
  , (SELECT "id" FROM public."report_template" WHERE "name" = 'Custom Report' LIMIT 1)
  , true
  , 0
  , ''
  , ''
)
ON CONFLICT ("name")
DO
  UPDATE SET "value" = (SELECT "id" FROM public."report_template" WHERE "name" = 'Custom Report' LIMIT 1);
