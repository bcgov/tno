DO $$
BEGIN

INSERT INTO public.connection (
  "name"
  , "description"
  , "is_enabled"
  , "connection_type"
  , "configuration"
  , "is_read_only"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'TNO 1.0 Database' -- 10
  , 'Connection to TNO 1.0 Database. Only for Content Migration.' -- description
  , true -- is_enabled
  , 8 -- connection_type - Database Connection
  , '{"hostname": "oracle_database", "port":1521, "sid": "", "password": "", "username": ""}' -- configuration
  , false -- is_read_only
  , 0 -- sort_order
  , ''
  , ''
);

END $$;
