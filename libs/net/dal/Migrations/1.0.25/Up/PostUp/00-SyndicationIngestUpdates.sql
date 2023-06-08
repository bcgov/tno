DO $$

DECLARE ingestSyndicationId INT := (SELECT id FROM public.ingest_type WHERE "name" = 'Syndication'); -- ingest_type_id
DECLARE onlineId INT := (SELECT id FROM public.product WHERE TRIM(name) IN ('Online', 'Online Print'));
DECLARE conPublicInternetId INT := (SELECT id FROM public.connection WHERE "name" = 'Public Internet'); -- connection_id

BEGIN

INSERT INTO public.schedule (
    "name"
    , "description"
    , "is_enabled"
    , "schedule_type"
    , "delay_ms"
    , "run_on"
    , "start_at"
    , "stop_at"
    , "repeat"
    , "run_on_week_days"
    , "run_on_months"
    , "day_of_month"
    , "created_by"
    , "updated_by"
    , "version"
) VALUES
-- ******************************************************
-- Syndication
-- ******************************************************
(
  'CBC | Aboriginal News' -- name
  , '' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 1000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'CBC | Top Stories News' -- name
  , 'CBC RSS Feed' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 5000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
), (
  'Link Newspaper' -- name
  , 'Link RSS Feed' -- description
  , true -- is_enabled
  , 1 -- schedule_type
  , 5000 -- delay_ms
  , NULL -- run_on
  , '00:00:00' -- start_at
  , '23:59:59' -- stop_at
  , 0 -- repeat
  , 127 -- run_on_week_days
  , 0 -- run_on_months
  , 0 -- day_of_month
  , '' -- created_by
  , '' -- updated_on
  , '0'
);

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
-- ******************************************************
-- Syndication
-- ******************************************************
(
  'CBC | Aboriginal News'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBCINDIGNEWS') -- source_id
  , 'CBCINDIGNEWS' -- topic
  , onlineId -- product_id
  , '{ "url":"https://www.cbc.ca/cmlink/rss-cbcaboriginal",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --source_connection_id
  , conPublicInternetId--destination_connection_id
  , ''
  , ''
), (
  'CBC | Top Stories'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBCO') -- source_id
  , 'CBCO' -- topic
  , onlineId -- product_id
  , '{ "url":"https://rss.cbc.ca/lineup/canada-britishcolumbia.xml",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --source_connection_id
  , conPublicInternetId--destination_connection_id
  , ''
  , ''
), (
  'Link Newspaper'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'LINK') -- source_id
  , 'LINK' -- topic
  , onlineId -- product_id
  , '{ "url":"http://thelinkpaper.ca/?feed=rss2",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --source_connection_id
  , conPublicInternetId--destination_connection_id
  , ''
  , ''
);

INSERT INTO public.ingest_schedule (
  "ingest_id"
  , "schedule_id"
  , "created_by"
  , "updated_by"
) VALUES (
  (SELECT id FROM public.ingest WHERE name = 'CBC | Aboriginal News')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC | Aboriginal News') -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'CBC | Top Stories')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'CBC | Top Stories News') -- schedule_id
  , ''
  , ''
), (
  (SELECT id FROM public.ingest WHERE name = 'Link Newspaper')  -- ingest_id
  , (SELECT id FROM public.schedule WHERE name = 'Link Newspaper') -- schedule_id
  , ''
  , ''
);

END $$;
