

DO $$
DECLARE FrontPageImageMediaTypeId INT;
DECLARE Query VARCHAR;
DECLARE Settings VARCHAR;
BEGIN

SELECT INTO FrontPageImageMediaTypeId id FROM public."media_type" where "name" = 'Front Page Images' limit 1;
SELECT INTO Query CONCAT('{"size":10,"sort":[],"query":{"bool":{"must":[{"range":{"publishedOn":{"gte":"now/d","time_zone":"US/Pacific"}}},{"terms":{"mediaTypeId":[', FrontPageImageMediaTypeId, ']}}]}}}');
SELECT INTO Settings CONCAT('{"searchUnpublished":false,"size":10,"dateOffset":0,"sourceIds":[],"mediaTypeIds":[', FrontPageImageMediaTypeId, '],"seriesIds":[],"contributorIds":[],"actions":[],"contentTypes":[],"tags":[],"sentiment":[],"sort":[]}');

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
  , Query::jsonb
  , Settings::jsonb
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT DO NOTHING;

END $$;
