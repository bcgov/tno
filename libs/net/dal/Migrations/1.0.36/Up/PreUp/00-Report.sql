DO $$
BEGIN

-- Store all the reports so that they can be restored.
CREATE TEMP TABLE _report (
  "id"
  , "name"
  , "description"
  , "report_type"
  , "owner_id"
  , "filter"
  , "template"
  , "settings"
  , "is_enabled"
  , "is_public"
  , "sort_order"
  , "created_on"
  , "created_by"
  , "updated_on"
  , "updated_by"
) AS
SELECT "id"
  , "name"
  , "description"
  , "report_type"
  , "owner_id"
  , "filter"
  , "template"
  , "settings"
  , "is_enabled"
  , "is_public"
  , "sort_order"
  , "created_on"
  , "created_by"
  , "updated_on"
  , "updated_by"
FROM public."report";

-- Delete all reports
-- This should cascade to all children (report_instance, report_instance_content)
DELETE FROM public."report";

END $$;
