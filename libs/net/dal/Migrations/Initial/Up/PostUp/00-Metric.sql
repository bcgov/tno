DO $$
BEGIN

INSERT INTO public.metric (
  "name"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'Monday' -- name
  , true
  , 1
  , ''
  , ''
), (
  'Tuesday' -- name
  , true
  , 2
  , ''
  , ''
), (
  'Wednesday' -- name
  , true
  , 3
  , ''
  , ''
), (
  'Thursday' -- name
  , true
  , 4
  , ''
  , ''
), (
  'Friday' -- name
  , true
  , 5
  , ''
  , ''
), (
  'Saturday' -- name
  , true
  , 6
  , ''
  , ''
), (
  'Sunday' -- name
  , true
  , 7
  , ''
  , ''
);

END $$;
