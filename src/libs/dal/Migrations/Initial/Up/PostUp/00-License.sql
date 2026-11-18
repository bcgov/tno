DO $$
BEGIN

INSERT INTO public.license (
  "name"
  , "description"
  , "is_enabled"
  , "ttl"
  , "created_by"
  , "updated_by"
) VALUES (
  'Never Expire'
  , 'Stored indefinitely'
  , true
  , 0 -- ttl
  , ''
  , ''
), (
  'Regular Expire'
  , 'Stored for three months'
  , true
  , 90 -- ttl
  , ''
  , ''
), (
  'Special Expire'
  , 'Stored for five months'
  , true
  , 150 -- ttl
  , ''
  , ''
), (
  'Weekly'
  , 'Stored for one week'
  , true
  , 7 -- ttl
  , ''
  , ''
), (
  'One year'
  , 'Stored for one year'
  , true
  , 360 -- ttl
  , ''
  , ''
), (
  'Five years'
  , 'Stored for five years'
  , true
  , 1800 -- ttl
  , ''
  , ''
), (
  'Ten years'
  , 'Stored for ten years'
  , true
  , 3600 -- ttl
  , ''
  , ''
);

END $$;
