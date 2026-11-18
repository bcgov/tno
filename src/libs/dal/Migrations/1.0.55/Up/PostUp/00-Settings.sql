DO $$
DECLARE morningReportId VARCHAR := COALESCE((SELECT "id" FROM public."report" WHERE "name" = 'Morning Report' LIMIT 1)::VARCHAR, '');

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
  'MorningReport' -- name
  , '' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , morningReportId -- value
  , '' -- created_by
  , '' -- updated_on
) ON CONFLICT DO NOTHING;

END $$;
