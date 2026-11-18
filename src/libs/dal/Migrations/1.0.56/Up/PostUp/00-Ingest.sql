DO $$

BEGIN

-- set all TNO Ingests to have a Continuous schedule as long as they
-- still have related records in the ingest_schedule and schedule tables
UPDATE public.ingest i
SET schedule_type = 1
where i.source_id = (select id from public.source where code = 'TNO')
and exists
    (select ingest_id
    from public.ingest_schedule i_s
    join public.schedule s on i_s.schedule_id = s.id
    where i_s.ingest_id = i.id);

END $$;
