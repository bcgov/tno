DO $$
BEGIN

INSERT INTO public.connection (
  "name"
  , "description"
  , "is_enabled"
  , "connection_type"
  , "configuration"
  , "is_read_only"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'Local Volume - Migrated AudioVideo'
  , 'A locally mapped volume location for storing migrated audio and video files.' -- description
  , true -- is_enabled
  , 0 -- connection_type - Local Volume
  , '{ "path": "audiovideo-migrated" }' -- configuration
  , false -- is_read_only
  , 0 -- sort_order
  , ''
  , ''
), (
  'Local Volume - Migrated Images'
  , 'A locally mapped volume location for storing migrated image files.' -- description
  , true -- is_enabled
  , 0 -- connection_type - Local Volume
  , '{ "path": "images-migrated" }' -- configuration
  , false -- is_read_only
  , 0 -- sort_order
  , ''
  , ''
),  (
  'Local Volume - Migrated Papers'
  , 'A locally mapped volume location for storing migrated paper files.' -- description
  , true -- is_enabled
  , 0 -- connection_type - Local Volume
  , '{"path": "papers-migrated"}' -- configuration
  , false -- is_read_only
  , 0 -- sort_order
  , ''
  , ''
),  (
  'Local Volume - Migrated Stories'
  , 'A locally mapped volume location for storing migrated story files.' -- description
  , true -- is_enabled
  , 0 -- connection_type - Local Volume
  , '{"path": "stories-migrated"}' -- configuration
  , false -- is_read_only
  , 0 -- sort_order
  , ''
  , ''
);

END $$;
