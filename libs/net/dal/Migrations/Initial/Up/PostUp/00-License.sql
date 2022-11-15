DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.license (
  "name"
  , "description"
  , "is_enabled"
  , "ttl"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Never Expire'
  , 'Stored indefinately'
  , true
  , 0 -- ttl
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Regular Expire'
  , 'Stored for three months'
  , true
  , 90 -- ttl
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Special Expire'
  , 'Stored for five months'
  , true
  , 150 -- ttl
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Weekly'
  , 'Stored for one week'
  , true
  , 7 -- ttl
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'One year'
  , 'Stored for one year'
  , true
  , 360 -- ttl
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Five years'
  , 'Stored for five years'
  , true
  , 1800 -- ttl
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ten years'
  , 'Stored for ten years'
  , true
  , 3600 -- ttl
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
