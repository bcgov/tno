DO $$
BEGIN

-- Create filters
INSERT INTO public."filter" (
  "name"
  , "description"
  , "is_enabled"
  , "sort_order"
  , "owner_id"
  , "query"
  , "settings"
  , "created_by"
  , "updated_by"
)
SELECT
  rf."name" -- name
  , '' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , rf."owner_id" -- owner_id
  , rf."query" -- query
  , '{}' -- settings
  , '' -- created_by
  , '' -- updated_by
FROM _report_filter rf;

END $$;
