DO $$
DECLARE frontPageFilterId VARCHAR := COALESCE((SELECT "id" FROM public."filter" WHERE "name" = 'Frontpages Report' LIMIT 1)::VARCHAR, '');

BEGIN

INSERT INTO public."setting" (
    "name"
    , "description"
    , "is_enabled"
    , "sort_order"
    , "value"
    , "created_by"
    , "updated_by"
) VALUES (
  'FrontpageFilter' -- name
  , 'This filter will be used to populate the frontpage session' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , frontPageFilterId -- value
  , '' -- created_by
  , '' -- updated_on
) ON CONFLICT DO NOTHING;

END $$;
