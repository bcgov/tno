DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.action (
  "name"
  , "is_enabled"
  , "value_label"
  , "value_type"
  , "default_value"
  , "sort_order"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Alert' -- name
  , true
  , '' -- value_label
  , 0 -- value_type
  , 'true' -- default_value
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Just In' -- name
  , true
  , '' -- value_label
  , 0 -- value_type
  , '' -- default_value
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Front Page' -- name
  , true
  , '' -- value_label
  , 0 -- value_type
  , '' -- default_value
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Top Story' -- name
  , true
  , '' -- value_label
  , 0 -- value_type
  , '' -- default_value
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'On Ticker' -- name
  , true
  , '' -- value_label
  , 0 -- value_type
  , '' -- default_value
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Non Qualified Subject' -- name
  , true
  , '' -- value_label
  , 0 -- value_type
  , '' -- default_value
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Commentary' -- name
  , true
  , 'Commentary Timeout' -- value_label
  , 1 -- value_type
  , '' -- default_value
  , 1 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
