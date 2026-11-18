DO $$

BEGIN

  INSERT INTO public.ingest (
    "name"
    , "description"
    , "is_enabled"
    , "ingest_type_id"
    , "source_id"
    , "topic"
    , "product_id"
    , "configuration"
    , "schedule_type"
    , "retry_limit"
    , "source_connection_id"
    , "destination_connection_id"
    , "created_by"
    , "updated_by"
  ) VALUES
  (
    'TNO 1.0 - AudioVideo Content - HISTORIC'
    , 'Ingest TNO 1.0 - AudioVideo Content - HISTORIC' -- description
    , true -- is_enabled
    , (SELECT "id" FROM public.ingest_type WHERE "name" = 'TNO-AudioVideo') -- ingest_type_id
    , (SELECT "id" FROM public.source WHERE "name" = 'TNO 1.0') -- source_id
    , 'TNO' -- topic
    , (SELECT "id" FROM public.product WHERE "name" = 'News Radio') -- product_id
    , '{ "post": true, "import": true, "importDateEnd": "2023-08-01 12:00:00 am" }' -- configuration
    , 1 -- schedule_type
    , 3 -- retry_limit
    , (SELECT "id" FROM public.connection WHERE "name" = 'TNO 1.0 Database') --source_connection_id
    , (SELECT "id" FROM public.connection WHERE "name" = 'Local Volume - Clips') --destination_connection_id
    , ''
    , ''
  ) ON CONFLICT DO NOTHING;

  INSERT INTO public.ingest (
    "name"
    , "description"
    , "is_enabled"
    , "ingest_type_id"
    , "source_id"
    , "topic"
    , "product_id"
    , "configuration"
    , "schedule_type"
    , "retry_limit"
    , "source_connection_id"
    , "destination_connection_id"
    , "created_by"
    , "updated_by"
  ) VALUES
(
  'TNO 1.0 - Print Content - HISTORIC'
  , 'Ingest TNO 1.0 - Print Content - HISTORIC' -- description
  , true -- is_enabled
  , (SELECT "id" FROM public.ingest_type WHERE "name" = 'TNO-PrintContent') -- ingest_type_id
  , (SELECT "id" FROM public.source WHERE "name" = 'TNO 1.0') -- source_id
  , 'TNO' -- topic
  , (SELECT "id" FROM public.product WHERE "name" = 'Online' OR "name" = 'Online ') -- product_id
  , '{ "post": true, "import": true, "importDateEnd": "2023-08-01 12:00:00 am" }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , (SELECT "id" FROM public.connection WHERE "name" = 'TNO 1.0 Database') --source_connection_id
  , (SELECT "id" FROM public.connection WHERE "name" = 'Local Volume - Papers') --destination_connection_id
  , ''
  , ''
  ) ON CONFLICT DO NOTHING;

  INSERT INTO public.ingest (
    "name"
    , "description"
    , "is_enabled"
    , "ingest_type_id"
    , "source_id"
    , "topic"
    , "product_id"
    , "configuration"
    , "schedule_type"
    , "retry_limit"
    , "source_connection_id"
    , "destination_connection_id"
    , "created_by"
    , "updated_by"
  ) VALUES
(
  'TNO 1.0 - Image Content - HISTORIC'
  , 'Ingest TNO 1.0 - Image Content - HISTORIC' -- description
  , true -- is_enabled
  , (SELECT "id" FROM public.ingest_type WHERE "name" = 'TNO-Image') -- ingest_type_id
  , (SELECT "id" FROM public.source WHERE "name" = 'TNO 1.0') -- source_id
  , 'TNO' -- topic
  , (SELECT "id" FROM public.product WHERE "name" = 'Front Page Images') -- product_id
  , '{ "post": true, "import": true, "importDateEnd": "2023-08-01 12:00:00 am" }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , (SELECT "id" FROM public.connection WHERE "name" = 'TNO 1.0 Database') --source_connection_id
  , (SELECT "id" FROM public.connection WHERE "name" = 'Local Volume - Images') --destination_connection_id
  , ''
  , ''
  ) ON CONFLICT DO NOTHING;

INSERT INTO public.ingest (
    "name"
    , "description"
    , "is_enabled"
    , "ingest_type_id"
    , "source_id"
    , "topic"
    , "product_id"
    , "configuration"
    , "schedule_type"
    , "retry_limit"
    , "source_connection_id"
    , "destination_connection_id"
    , "created_by"
    , "updated_by"
  ) VALUES
(
  'TNO 1.0 - Story Content - HISTORIC'
  , 'Ingest TNO 1.0 - Story Content - HISTORIC' -- description
  , true -- is_enabled
  , (SELECT "id" FROM public.ingest_type WHERE "name" = 'TNO-Story') -- ingest_type_id
  , (SELECT "id" FROM public.source WHERE "name" = 'TNO 1.0') -- source_id
  , 'TNO' -- topic
  , (SELECT "id" FROM public.product WHERE "name" = 'CP Wire') -- product_id
  , '{ "post": true, "import": true, "importDateEnd": "2023-08-01 12:00:00 am" }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , (SELECT "id" FROM public.connection WHERE "name" = 'TNO 1.0 Database') --source_connection_id
  , (SELECT "id" FROM public.connection WHERE "name" = 'Local Volume - Streams') --destination_connection_id
  , ''
  , ''
) ON CONFLICT DO NOTHING;

END $$;
