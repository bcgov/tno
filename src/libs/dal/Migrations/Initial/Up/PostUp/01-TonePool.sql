DO $$
BEGIN

INSERT INTO public.tone_pool (
  "name"
  , "is_enabled"
  , "owner_id"
  , "is_public"
  , "created_by"
  , "updated_by"
) VALUES (
  'Default'
  , true
  , 1
  , true
  , ''
  , ''
);

END $$;
