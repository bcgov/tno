DO $$
BEGIN

-- Re-insert the reports.
INSERT INTO public."report" (
  "name"
  , "description"
  , "report_type"
  , "owner_id"
  , "filter"
  , "report_template_id"
  , "settings"
  , "is_enabled"
  , "is_public"
  , "sort_order"
  , "created_on"
  , "created_by"
  , "updated_on"
  , "updated_by"
) SELECT
  r."name"
  , r."description"
  , r."report_type"
  , r."owner_id"
  , r."filter"
  , (SELECT "id" FROM public."report_template" WHERE "name"=r."name")-- report_template_id
  , r."settings"
  , r."is_enabled"
  , r."is_public"
  , r."sort_order"
  , r."created_on"
  , r."created_by"
  , r."updated_on"
  , r."updated_by"
FROM _report r;

DROP TABLE _report;

END $$;
