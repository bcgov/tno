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
    'TNO 1.0 - AudioVideo Content - RECENTLY-PUBLISHED'
    , 'Ingest TNO 1.0 - AudioVideo Content - RECENTLY-PUBLISHED' -- description
    , true -- is_enabled
    , (SELECT "id" FROM public.ingest_type WHERE "name" = 'TNO-AudioVideo') -- ingest_type_id
    , (SELECT "id" FROM public.source WHERE "name" = 'TNO 1.0') -- source_id
    , 'TNO' -- topic
    , (SELECT "id" FROM public.product WHERE "name" = 'News Radio') -- product_id
  , '{ "post": true, "import": false, "importMigrationType": "RecentlyPublished", "migrationTimeOffsetInHours": 48 }' -- configuration
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
  'TNO 1.0 - Print Content - RECENTLY-PUBLISHED'
  , 'Ingest TNO 1.0 - Print Content - RECENTLY-PUBLISHED' -- description
  , true -- is_enabled
  , (SELECT "id" FROM public.ingest_type WHERE "name" = 'TNO-PrintContent') -- ingest_type_id
  , (SELECT "id" FROM public.source WHERE "name" = 'TNO 1.0') -- source_id
  , 'TNO' -- topic
  , (SELECT "id" FROM public.product WHERE "name" = 'Online' OR "name" = 'Online ') -- product_id
  , '{ "post": true, "import": false, "importMigrationType": "RecentlyPublished", "migrationTimeOffsetInHours": 48 }' -- configuration
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
  'TNO 1.0 - Image Content - RECENTLY-PUBLISHED'
  , 'Ingest TNO 1.0 - Image Content - RECENTLY-PUBLISHED' -- description
  , true -- is_enabled
  , (SELECT "id" FROM public.ingest_type WHERE "name" = 'TNO-Image') -- ingest_type_id
  , (SELECT "id" FROM public.source WHERE "name" = 'TNO 1.0') -- source_id
  , 'TNO' -- topic
  , (SELECT "id" FROM public.product WHERE "name" = 'Front Page Images') -- product_id
  , '{ "post": true, "import": false, "importMigrationType": "RecentlyPublished", "migrationTimeOffsetInHours": 48 }' -- configuration
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
  'TNO 1.0 - Story Content - RECENTLY-PUBLISHED'
  , 'Ingest TNO 1.0 - Story Content - RECENTLY-PUBLISHED' -- description
  , true -- is_enabled
  , (SELECT "id" FROM public.ingest_type WHERE "name" = 'TNO-Story') -- ingest_type_id
  , (SELECT "id" FROM public.source WHERE "name" = 'TNO 1.0') -- source_id
  , 'TNO' -- topic
  , (SELECT "id" FROM public.product WHERE "name" = 'CP Wire') -- product_id
  , '{ "post": true, "import": false, "importMigrationType": "RecentlyPublished", "migrationTimeOffsetInHours": 48 }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , (SELECT "id" FROM public.connection WHERE "name" = 'TNO 1.0 Database') --source_connection_id
  , (SELECT "id" FROM public.connection WHERE "name" = 'Local Volume - Streams') --destination_connection_id
  , ''
  , ''
) ON CONFLICT DO NOTHING;

END $$;
