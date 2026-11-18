DO $$

--DECLARE openshiftId INT := (SELECT "id" FROM public.data_location WHERE Name = 'Openshift'); -- data_location_id
--DECLARE conDatabaseConnectionId INT := (SELECT id FROM public.connection WHERE "name" = 'TNO 1.0 Database'); -- source_connection_id

BEGIN

INSERT INTO public.ingest_data_location (
  "ingest_id"
  , "data_location_id"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
)
SELECT
  "id"
  , (SELECT "id" FROM public.data_location WHERE Name = 'Openshift') -- data_location_id
  , ''
  , CURRENT_TIMESTAMP
  , ''
  , CURRENT_TIMESTAMP
FROM public.ingest
WHERE "source_connection_id" = (SELECT id FROM public.connection WHERE "name" = 'TNO 1.0 Database');

END $$;
