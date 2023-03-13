DO $$
BEGIN

INSERT INTO public."report" (
  "name"
  , "description"
  , "report_type"
  , "filter"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'Morning Report' -- name
  , 'The morning report contains stories linked to topics for the day.' -- description
  , 1 -- report_type
  , '{}'
  , true -- is_enabled
  , 0 -- sort_order
  , ''
  , ''
);

END $$;
