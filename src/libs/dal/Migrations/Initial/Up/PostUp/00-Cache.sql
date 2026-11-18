DO $$
BEGIN

INSERT INTO public.cache (
  "key"
  , "value"
  , "description"
  , "created_by"
  , "updated_by"
) VALUES (
  'claims' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'roles' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'users' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'actions' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'source_actions' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'metrics' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'categories' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'tags' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'tone_pools' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'products' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'ingest_types' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'licenses' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'series' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'connections' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'ingests' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'sources' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
), (
  'lookups' -- key
  , gen_random_uuid() -- value
  , '' -- description
  , ''
  , ''
);

END $$;
