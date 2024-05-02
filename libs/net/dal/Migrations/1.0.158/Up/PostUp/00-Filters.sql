

DO $$
BEGIN

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
) VALUES (
  'DAILY - Front page images' -- name
  , 'Returns daily paper front page images for today' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , 1 -- owner_id
  , '{"size":10,"sort":[],"query":{"bool":{"must":[{"range":{"publishedOn":{"gte":"now/d","time_zone":"US/Pacific"}}},{"terms":{"mediaTypeId":[11]}}]}}}' -- query
  , '{"searchUnpublished":false,"size":10,"dateOffset":0,"sourceIds":[],"mediaTypeIds":[11],"seriesIds":[],"contributorIds":[],"actions":[],"contentTypes":[],"tags":[],"sentiment":[],"sort":[]}' -- settings
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT DO NOTHING;

END $$;
