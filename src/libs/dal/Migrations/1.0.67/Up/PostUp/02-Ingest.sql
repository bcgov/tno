DO $$

BEGIN

update public.ingest
set destination_connection_id = (SELECT "id" FROM public.connection WHERE "name" = 'Local Volume - Migrated AudioVideo')
where ingest_type_id = (SELECT "id" FROM public.ingest_type WHERE "name" = 'TNO-AudioVideo');

update public.ingest
set destination_connection_id = (SELECT "id" FROM public.connection WHERE "name" = 'Local Volume - Migrated Papers')
where ingest_type_id = (SELECT "id" FROM public.ingest_type WHERE "name" = 'TNO-PrintContent');

update public.ingest
set destination_connection_id = (SELECT "id" FROM public.connection WHERE "name" = 'Local Volume - Migrated Images')
where ingest_type_id = (SELECT "id" FROM public.ingest_type WHERE "name" = 'TNO-Image');

update public.ingest
set destination_connection_id = (SELECT "id" FROM public.connection WHERE "name" = 'Local Volume - Migrated Stories')
where ingest_type_id = (SELECT "id" FROM public.ingest_type WHERE "name" = 'TNO-Story');

END $$;
