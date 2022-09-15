DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.connection (
  "name"
  , "description"
  , "is_enabled"
  , "connection_type"
  , "configuration"
  , "is_read_only"
  , "sort_order"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'None' -- 1
  , 'Connection settings for this location are not required.  There are no physical files that need to be stored.' -- description
  , true -- is_enabled
  , 0 -- connection_type - Local Volume
  , '{}' -- configuration
  , false -- is_read_only
  , -1 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Local Volume - Streams' -- 2
  , 'A locally mapped volume location for streaming files.' -- description
  , true -- is_enabled
  , 0 -- connection_type - Local Volume
  , '{ "path": "capture" }' -- configuration
  , false -- is_read_only
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Local Volume - Clips' -- 3
  , 'A locally mapped volume location for clip files' -- description
  , true -- is_enabled
  , 0 -- connection_type - Local Volume
  , '{ "path": "clips" }' -- configuration
  , false -- is_read_only
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Internet' -- 4
  , 'Streaming data source' -- description
  , true -- is_enabled
  , 3 -- connection_type - HTTP
  , '{}' -- configuration
  , true -- is_read_only
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'SSH - Newspaper Upload' -- 5
  , 'Meltwater and Blacks News Group upload files to this location.' -- description
  , true -- is_enabled
  , 7 -- connection_type - SSH
  , '{}' -- configuration
  , true -- is_read_only
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
