DO $$
BEGIN

INSERT INTO public."report" (
  "name"
  , "owner_id"
  , "description"
  , "report_type"
  , "filter"
  , "template"
  , "is_public"
  , "settings"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) SELECT
  r."name"
  , 1 -- owner_id
  , r."description"
  , r."report_type"
  , r."filter"
  , '' -- template
  , true -- is_public
  , '{}' -- settings
  , r."is_enabled"
  , r."sort_order"
  , r."created_by"
  , r."updated_by"
FROM _report r;

DROP TABLE _report;

END $$;
