DO $$

DECLARE srcTNOId INT := (SELECT id FROM public.source WHERE "name" = 'TNO 1.0'); -- source_id

DECLARE conDatabaseConnectionId INT := (SELECT id FROM public.connection WHERE "name" = 'TNO 1.0 Database'); -- source_connection_id

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
  'TNO 1.0 - Snippet Content'
  , 'Ingest TNO 1.0 - Snippet Content' -- description
  , true -- is_enabled
  , (SELECT id FROM public.ingest_type WHERE name = 'TNO-Snippet') -- ingest_type_id
  , srcTNOId -- source_id
  , 'TNO' -- topic
  , (SELECT id FROM public.product WHERE name = 'News Radio') -- product_id
  , '{ "post": true,
      "import": true }' -- configuration
  , 0 -- schedule_type
  , 3 -- retry_limit
  , conDatabaseConnectionId --destination_connection_id
  , (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Clips') --destination_connection_id
  , ''
  , ''
),(
  'TNO 1.0 - Print Content'
  , 'Ingest TNO 1.0 - Print Content' -- description
  , true -- is_enabled
  , (SELECT id FROM public.ingest_type WHERE name = 'TNO-PrintContent') -- ingest_type_id
  , srcTNOId -- source_id
  , 'TNO' -- topic
  , (SELECT id FROM public.product WHERE name = 'Online Print') -- product_id
  , '{ "post": true,
      "import": true }' -- configuration
  , 0 -- schedule_type
  , 3 -- retry_limit
  , conDatabaseConnectionId --destination_connection_id
  , (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Papers') --destination_connection_id
  , ''
  , ''
),(
  'TNO 1.0 - Image Content'
  , 'Ingest TNO 1.0 - Image Content' -- description
  , true -- is_enabled
  , (SELECT id FROM public.ingest_type WHERE name = 'TNO-Image') -- ingest_type_id
  , srcTNOId -- source_id
  , 'TNO' -- topic
  , (SELECT id FROM public.product WHERE name = 'Front Page') -- product_id
  , '{ "post": true,
      "import": true }' -- configuration
  , 0 -- schedule_type
  , 3 -- retry_limit
  , conDatabaseConnectionId --destination_connection_id
  , (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Images') --destination_connection_id
  , ''
  , ''
),(
  'TNO 1.0 - Story Content'
  , 'Ingest TNO 1.0 - Story Content' -- description
  , true -- is_enabled
  , (SELECT id FROM public.ingest_type WHERE name = 'TNO-Story') -- ingest_type_id
  , srcTNOId -- source_id
  , 'TNO' -- topic
  , (SELECT id FROM public.product WHERE name = 'CP Wire') -- product_id
  , '{ "post": true,
      "import": true }' -- configuration
  , 0 -- schedule_type
  , 3 -- retry_limit
  , conDatabaseConnectionId --destination_connection_id
  , (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Streams') --destination_connection_id
  , ''
  , ''
);

END $$;
