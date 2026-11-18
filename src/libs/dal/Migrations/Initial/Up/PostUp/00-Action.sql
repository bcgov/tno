DO $$
BEGIN

INSERT INTO public.action (
  "name"
  , "is_enabled"
  , "value_label"
  , "value_type"
  , "default_value"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'Alert' -- name - 1
  , true -- is_enabled
  , '' -- value_label
  , 0 -- value_type
  , 'true' -- default_value
  , 0 -- sort_order
  , ''
  , ''
), (
  'Just In' -- name - 2
  , true -- is_enabled
  , '' -- value_label
  , 0 -- value_type
  , '' -- default_value
  , 0 -- sort_order
  , ''
  , ''
), (
  'Front Page' -- name - 3
  , true -- is_enabled
  , '' -- value_label
  , 0 -- value_type
  , '' -- default_value
  , 0 -- sort_order
  , ''
  , ''
), (
  'Top Story' -- name - 4
  , true -- is_enabled
  , '' -- value_label
  , 0 -- value_type
  , '' -- default_value
  , 0 -- sort_order
  , ''
  , ''
), (
  'On Ticker' -- name - 5
  , false -- is_enabled
  , '' -- value_label
  , 0 -- value_type
  , '' -- default_value
  , 0 -- sort_order
  , ''
  , ''
), (
  'Non Qualified Subject' -- name - 6
  , true -- is_enabled
  , '' -- value_label
  , 0 -- value_type
  , '' -- default_value
  , 0 -- sort_order
  , ''
  , ''
), (
  'Commentary' -- name - 7
  , true -- is_enabled
  , 'Commentary Timeout' -- value_label
  , 1 -- value_type
  , '' -- default_value
  , 1 -- sort_order
  , ''
  , ''
);

END $$;
