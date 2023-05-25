DO $$
BEGIN

INSERT INTO public.source (
  "name"
  , "code"
  , "short_name"
  , "description"
  , "is_enabled"
  , "auto_transcribe"
  , "disable_transcribe"
  , "license_id"
  , "product_id"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'TNO 1.0'
  , 'TNO'
  , '' -- short_name
  , 'TNO 1.0 Database content as source' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , -1 -- sort_order
  , ''
  , ''
);

END $$;
