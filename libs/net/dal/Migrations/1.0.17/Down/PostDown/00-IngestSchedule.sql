DO $$
DECLARE conDatabaseConnectionId INT := (SELECT id FROM public.connection WHERE "name" = 'TNO 1.0 Database'); -- source_connection_id

BEGIN

delete from public.ingest_schedule
where ingest_id in
(select ingest_id from public.ingest where destination_connection_id = destination_connection_id);

END $$;
