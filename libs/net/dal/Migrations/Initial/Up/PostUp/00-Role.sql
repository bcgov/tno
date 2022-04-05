DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.role (
  "name"
  , "key"
  , "is_enabled"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Administrator' -- name
  , 'c89a9f73-eaab-4c4f-8c90-f963de0c5854' -- key
  , true -- isEnabled
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Editor' -- name
  , 'd5786a5b-4f71-46f1-9c0c-f6b69ee741b1' -- key
  , true -- isEnabled
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Subscriber' -- name
  , '250eaa7f-67c9-4a60-b453-8ca790d569f5' -- key
  , true -- isEnabled
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
