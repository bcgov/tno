DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.claim (
  "name"
  , "key"
  , "is_enabled"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'administrator' -- name
  , 'db055cc6-b31c-42c4-99b5-5e519c31c8ab' -- key
  , true -- isEnabled
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'editor' -- name
  , 'c374cbb1-7eda-4259-8f74-cd6c2287e32b' -- key
  , true -- isEnabled
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'subscriber' -- name
  , '4818b135-034e-40d8-bd9c-8df2573ce9e0' -- key
  , true -- isEnabled
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
