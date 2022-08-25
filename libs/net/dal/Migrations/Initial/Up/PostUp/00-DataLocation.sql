DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.data_location (
  "name"
  , "description"
  , "is_enabled"
  , "location_type"
  , "connection"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Default' -- 1
  , 'If content does not have a configured data source, then it will be uploaded at this location.'
  , true
  , 0
  , '{}'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Local Volume' -- 2
  , 'A locally mapped volume location.  This provides less network traffic, but depends on drive read/write speeds.'
  , true
  , 0
  , '{}'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Remote Volume' -- 3
  , 'A remotely mapped volume location.  This requires more network traffic, but can improve drive read/write speeds.'
  , true
  , 1
  , '{}'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'NAS' -- 4
  , 'Network Attached Storage location.'
  , true
  , 2
  , '{}'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Internet' -- 5
  , 'Content belongs on some 3rd party website through a URL.'
  , true
  , 3
  , '{}'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'FTP' -- 6
  , 'Insecure FTP location.'
  , true
  , 4
  , '{}'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'SFTP' -- 7
  , 'Securet FTP location.'
  , true
  , 5
  , '{}'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Azure' -- 8
  , 'Microsoft Azure cloud storage.'
  , true
  , 6
  , '{}'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;