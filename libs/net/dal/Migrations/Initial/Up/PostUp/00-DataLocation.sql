DO $$
BEGIN

INSERT INTO public.data_location (
  "name"
  , "description"
  , "is_enabled"
  , "connection_id"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'Openshift' -- 1
  , 'Openshift silver cluster.' -- description
  , true -- is_enabled
  , NULL -- connection_id - Local Volume
  , 0 -- sort_order
  , ''
  , ''
), (
  'Server Room' -- 1
  , 'Server room containing hard line connections.' -- description
  , true -- is_enabled
  , (SELECT id FROM public.connection WHERE name = 'NAS - Server Room') -- connection_id - Local Volume
  , 0 -- sort_order
  , ''
  , ''
);

END $$;
