DO $$
BEGIN

-- Initialize settings.  These will need to be configured correctly for each environment manually.

INSERT INTO public."setting" (
  "name"
  , "description"
  , "value"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'MediaTypesAllSources' -- name
  , 'These media types will not consider source_media_type_search_mapping table and filter just by Media Type, not considering sources in filter.' -- description
  , '1,2,3,4,5,6,10'
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT("name") DO UPDATE
SET "value" = '1,2,3,4,5,6,10';

END $$;