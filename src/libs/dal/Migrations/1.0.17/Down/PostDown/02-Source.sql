DO $$
BEGIN

delete from public.source
where name ='TNO 1.0'
    and code = 'TNO'
    and description = 'TNO 1.0 Database content as source';

END $$;
