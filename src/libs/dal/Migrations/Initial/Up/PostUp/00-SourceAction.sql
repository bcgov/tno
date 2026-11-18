DO $$
BEGIN

INSERT INTO public.source_action (
  "name"
  , "is_enabled"
  , "created_by"
  , "updated_by"
) VALUES (
  'CBRA' -- name
  , true
  , ''
  , ''
), (
  'TV Archive' -- name
  , true
  , ''
  , ''
), (
  'Use in Analysis' -- name
  , true
  , ''
  , ''
), (
  'Use in Event of Day' -- name
  , true
  , ''
  , ''
);

END $$;
