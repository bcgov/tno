DO $$

BEGIN

UPDATE public.ingest i
set configuration = jsonb_set(configuration, '{importMigrationType}', '"Historic"')
where i.source_id = (select id from public.source where code = 'TNO')
and name like '% - HISTORIC';

UPDATE public.ingest i
set configuration = jsonb_set(configuration, '{importMigrationType}', '"Recent"')
where i.source_id = (select id from public.source where code = 'TNO')
and name like '% - RECENT';

END $$;
