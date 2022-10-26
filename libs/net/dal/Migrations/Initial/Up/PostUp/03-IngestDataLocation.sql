DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
DECLARE openshiftId INT := (SELECT "id" FROM public.data_location WHERE Name = 'Openshift'); -- data_location_id
BEGIN

INSERT INTO public.ingest_data_location (
  "ingest_id"
  , "data_location_id"
  , "created_by_id"
  , "created_by"
  , "created_on"
  , "updated_by_id"
  , "updated_by"
  , "updated_on"
)
SELECT
  "id"
  , openshiftId -- data_location_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
FROM public.ingest;

END $$;
