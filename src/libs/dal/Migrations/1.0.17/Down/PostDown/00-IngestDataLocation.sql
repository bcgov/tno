DO $$

DECLARE conDatabaseConnectionId INT := (SELECT id FROM public.connection WHERE "name" = 'TNO 1.0 Database'); -- source_connection_id

BEGIN

delete FROM public.ingest_data_location
WHERE data_location_id = conDatabaseConnectionId;

END $$;
