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
  'Local Volume - Images' -- 3
  , 'A locally mapped volume location for image files' -- description
  , true -- is_enabled
  , 0 -- connection_type - Local Volume
  , '{ "path": "images" }' -- configuration
  , false -- is_read_only
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
),  (
  'Local Volume - Papers' -- 4
  , 'This is the location to which newspaper import files are placed when they are retrieved from the remote server.' -- description
  , true -- is_enabled
  , 0 -- connection_type - Local Volume
  , '{"path": "papers"}' -- configuration
  , false -- is_read_only
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Public Internet' -- 5
  , 'Internet based data sources that do not require security (i.e. HTML, Streaming)' -- description
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
  'SSH - Newspaper Upload' -- 6
  , 'Meltwater and Blacks News Group upload files to this location.' -- description
  , true -- is_enabled
  , 7 -- connection_type - SSH
  , '{"path":"/dsk98","username":"","hostname":"scharnhorst.tno.gov.bc.ca","keyFileName":"id_rsa"}' -- configuration
  , true -- is_read_only
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'SSH - Globe Newspaper Upload' -- 6
  , 'Globe and Mail upload files to this location.' -- description
  , true -- is_enabled
  , 7 -- connection_type - SSH
  , '{"path":"/","username":"","hostname":"gamdelivery.globeandmail.ca","password": ""}' -- configuration
  , true -- is_read_only
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'NAS - Server Room' -- 6
  , 'Server room NAS access point' -- description
  , true -- is_enabled
  , 7 -- connection_type - SSH
  , '{"path":"/home/admin/data","username":"","hostname":"tno-capture-01.local","password": ""}' -- configuration
  , true -- is_read_only
  , 0 -- sort_order
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
