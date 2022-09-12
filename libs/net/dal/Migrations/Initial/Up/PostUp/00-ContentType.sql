DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.content_type (
  "name"
  , "description"
  , "sort_order"
  , "is_enabled"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'None'
  , 'Does not generate content.'
  , 0
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Snippet'
  , 'Audio/Video content representing a clip from a larger AV stream.'
  , 1
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Print'
  , 'Newspaper content that came from a physical source.'
  , 1
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Internet'
  , 'Content originated from the internet and contains a URL.'
  , 1
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Frontpage'
  , 'Daily frontpage images of newspapers.'
  , 1
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
