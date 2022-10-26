DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.data_location (
  "name"
  , "description"
  , "is_enabled"
  , "connection_id"
  , "sort_order"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Openshift' -- 1
  , 'Openshift silver cluster.' -- description
  , true -- is_enabled
  , NULL -- connection_id - Local Volume
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Server Room' -- 1
  , 'Server room containing hard line connections.' -- description
  , true -- is_enabled
  , (SELECT id FROM public.connection WHERE name = 'NAS - Server Room') -- connection_id - Local Volume
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
