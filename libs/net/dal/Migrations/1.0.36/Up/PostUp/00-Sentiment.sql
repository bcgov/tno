DO $$
BEGIN

INSERT INTO public."sentiment" (
  "name"
  , "description"
  , "is_enabled"
  , "sort_order"
  , "value"
  , "rate"
  , "created_by"
  , "updated_by"
) VALUES (
  '-5' -- name
  , '' -- description
  , TRUE -- is_enabled
  , 0 -- sort_order
  , -5 -- value
  , 0.05 -- rate
  , '' -- created_by
  , '' -- updated_by
), (
  '-4' -- name
  , '' -- description
  , TRUE -- is_enabled
  , 1 -- sort_order
  , -4 -- value
  , 0.1 -- rate
  , '' -- created_by
  , '' -- updated_by
), (
  '-3' -- name
  , '' -- description
  , TRUE -- is_enabled
  , 2 -- sort_order
  , -3 -- value
  , 0.2 -- rate
  , '' -- created_by
  , '' -- updated_by
), (
  '-2' -- name
  , '' -- description
  , TRUE -- is_enabled
  , 3 -- sort_order
  , -2 -- value
  , 0.3 -- rate
  , '' -- created_by
  , '' -- updated_by
), (
  '-1' -- name
  , '' -- description
  , TRUE -- is_enabled
  , 4 -- sort_order
  , -1 -- value
  , 0.4 -- rate
  , '' -- created_by
  , '' -- updated_by
), (
  '0' -- name
  , '' -- description
  , TRUE -- is_enabled
  , 5 -- sort_order
  , 0 -- value
  , 0.5 -- rate
  , '' -- created_by
  , '' -- updated_by
), (
  '1' -- name
  , '' -- description
  , TRUE -- is_enabled
  , 6 -- sort_order
  , 1 -- value
  , 0.6 -- rate
  , '' -- created_by
  , '' -- updated_by
), (
  '2' -- name
  , '' -- description
  , TRUE -- is_enabled
  , 7 -- sort_order
  , 2 -- value
  , 0.7 -- rate
  , '' -- created_by
  , '' -- updated_by
), (
  '3' -- name
  , '' -- description
  , TRUE -- is_enabled
  , 8 -- sort_order
  , 3 -- value
  , 0.8 -- rate
  , '' -- created_by
  , '' -- updated_by
), (
  '4' -- name
  , '' -- description
  , TRUE -- is_enabled
  , 9 -- sort_order
  , 4 -- value
  , 0.9 -- rate
  , '' -- created_by
  , '' -- updated_by
), (
  '5' -- name
  , '' -- description
  , TRUE -- is_enabled
  , 10 -- sort_order
  , 5 -- value
  , 1 -- rate
  , '' -- created_by
  , '' -- updated_by
);

END $$;
