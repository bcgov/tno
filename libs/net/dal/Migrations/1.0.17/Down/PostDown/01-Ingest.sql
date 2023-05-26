DO $$

DECLARE srcTNOId INT := (SELECT id FROM public.source WHERE "name" = 'TNO 1.0'); -- source_id

DECLARE conDatabaseConnectionId INT := (SELECT id FROM public.connection WHERE "name" = 'TNO 1.0 Database'); -- source_connection_id

BEGIN

delete from public.ingest
where source_connection_id = conDatabaseConnectionId
  and source_id = srcTNOId;

END $$;
