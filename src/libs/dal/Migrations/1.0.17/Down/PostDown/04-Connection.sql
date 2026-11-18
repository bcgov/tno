DO $$
BEGIN

delete from public.connection
where "name" = 'TNO 1.0 Database'
  and  "description" = 'Connection to TNO 1.0 Database. Only for Content Migration.'
  and  "connection_type" = 8; -- connection_type - Database Connection

END $$;
