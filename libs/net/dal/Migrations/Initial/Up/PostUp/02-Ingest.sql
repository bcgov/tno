DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
DECLARE syndicationId INT := (SELECT id FROM public.ingest_type WHERE Name = 'Syndication'); -- ingest_type_id
DECLARE videoId INT := (SELECT id FROM public.ingest_type WHERE Name = 'Video'); -- ingest_type_id
DECLARE audioId INT := (SELECT id FROM public.ingest_type WHERE Name = 'Audio'); -- ingest_type_id

DECLARE wireId INT := (SELECT id FROM public.product WHERE Name = 'Wire'); -- product_id
DECLARE weeklyPrintId INT := (SELECT id FROM public.product WHERE Name = 'Weekly Print'); -- product_id
DECLARE talkRadioId INT := (SELECT id FROM public.product WHERE Name = 'Talk Radio'); -- product_id
DECLARE videoNewsId INT := (SELECT id FROM public.product WHERE Name = 'Video News'); -- product_id

DECLARE conNoneId INT := (SELECT id FROM public.connection WHERE Name = 'None'); -- connection_id
DECLARE conLocalStreamsId INT := (SELECT id FROM public.connection WHERE Name = 'Local Volume - Streams'); -- connection_id
DECLARE conLocalClipsId INT := (SELECT id FROM public.connection WHERE Name = 'Local Volume - Clips'); -- connection_id
DECLARE conLocalImagesId INT := (SELECT id FROM public.connection WHERE Name = 'Local Volume - Images'); -- connection_id
DECLARE conPublicInternetId INT := (SELECT id FROM public.connection WHERE Name = 'Public Internet'); -- connection_id
DECLARE conSSHId INT := (SELECT id FROM public.connection WHERE Name = 'SSH - Newspaper Upload'); -- connection_id
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
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES
-- ******************************************************
-- Syndication
-- ******************************************************
(
  'Daily Hive'
  , '' -- description
  , true -- is_enabled
  , syndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'DAILYHIVE') -- source_id
  , 'DAILYHIVE' -- topic
  , wireId -- product_id
  , '{ "url":"http://dailyhive.com/feed/vancouver",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 0 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Georgia Straight'
  , '' -- description
  , true -- is_enabled
  , syndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'GEORGIA STRAIGHT') -- source_id
  , 'GEORGIA STRAIGHT' -- topic
  , wireId -- product_id
  , '{ "url":"http://www.straight.com/xml/feeds/bcg/news",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Castanet'
  , '' -- description
  , true -- is_enabled
  , syndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CASTANET') -- source_id
  , 'CASTANET' -- topic
  , wireId -- product_id
  , '{ "url":"https://www.castanet.net/rss/topheadlines.xml",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'iPolitics'
  , '' -- description
  , true -- is_enabled
  , syndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'IPOLY') -- source_id
  , 'IPOLY' -- topic
  , wireId -- product_id
  , '{ "url":"http://www.ipolitics.ca/custom-feeds/bc-gov-feed.php",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Business in Vancouver'
  , '' -- description
  , true -- is_enabled
  , syndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'BIV') -- source_id
  , 'BIV' -- topic
  , wireId -- product_id
  , '{ "url":"http://biv.com/rss",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Globe and Mail'
  , '' -- description
  , true -- is_enabled
  , syndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'GLOBE') -- source_id
  , 'GLOBE' -- topic
  , wireId -- product_id
  , '{ "url":"sftp://gamdelivery.globeandmail.ca/", "username":"", "password":"",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 0 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Prince George Citizen'
  , '' -- description
  , true -- is_enabled
  , syndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'PGC') -- source_id
  , 'PGC' -- topic
  , wireId -- product_id
  , '{ "url":"http://library.pressdisplay.com/test/qa/Services/AdvancedSearchRssHandler.ashx?srchText=%2a&srchnewspaper=7254&extended=false",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC Online'
  , '' -- description
  , true -- is_enabled
  , syndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBCO') -- source_id
  , 'CBCO' -- topic
  , wireId -- product_id
  , '{ "url":"https://www.cbc.ca/cmlink/rss-topstories",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Canadian Press Wire'
  , ''
  , true -- is_enabled
  , syndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CPNEWS') -- source_id
  , 'CPNEWS' -- topic
  , wireId -- product_id
  , '{ "url":"http://www.commandnews.com/fpweb/fp.dll/$bc-rss/htm/rss/x_searchlist.htm/_drawerid/!default_bc-rss/_profileid/rss/_iby/daj/_iby/daj/_svc/cp_pub/_k/XQkKHjnAUpumRfdr",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "fetchContent": true,
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Victoria Buzz'
  , '' -- description
  , true -- is_enabled
  , syndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'VBUZZ') -- source_id
  , 'VBUZZ' -- topic
  , wireId -- product_id
  , '{ "url":"http://www.victoriabuzz.com/feed/",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Orca'
  , '' -- description
  , true -- is_enabled
  , syndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'ORCA') -- source_id
  , 'ORCA' -- topic
  , wireId -- product_id
  , '{ "url":"https://theorca.ca/feed/",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Narwhal'
  , '' -- description
  , true -- is_enabled
  , syndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'NAR') -- source_id
  , 'NAR' -- topic
  , wireId -- product_id
  , '{ "url":"https://thenarwhal.ca/feed/rss2",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Infotel'
  , '' -- description
  , true -- is_enabled
  , syndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'INFOTEL') -- source_id
  , 'INFOTEL' -- topic
  , wireId -- product_id
  , '{ "url":"https://infotel.ca/govbcrssfeed",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
),

-- ******************************************************
-- Video
-- ******************************************************
(
  'CBC News'
  , 'Stay on top of British Columbia with the latest in news, weather, sports and interviews.' -- description
  , true -- is_enabled
  , videoId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBC') -- source_id
  , 'CBC' -- topic
  , videoNewsId -- product_id
  , '{ "serviceType": "stream",
      "url": "https://cbcrclinear-tor.akamaized.net/hls/live/2042769/geo_allow_ca/CBCRCLINEAR_TOR_15/master4.m3u8",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "fileName": "{schedule.Name}.mp4",
      "post": true,
      "import": true }' -- configuration
  , 3 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalClipsId -- destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
),

-- ******************************************************
-- Audio
-- ******************************************************
(
  'CBC Victoria - Stream'
  , '' -- description
  , true -- is_enabled
  , audioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBCV') -- source_id
  , 'CBCV' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://cbcmp3.ic.llnwd.net/stream/cbcmp3_cbc_r1_vcr",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC Victoria - Clips'
  , '' -- description
  , true -- is_enabled
  , audioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBCV') -- source_id
  , 'CBCV' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"clip",
      "keepChecking":true,
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 3 -- schedule_type
  , 3 -- retry_limit
  , conLocalStreamsId -- source_connection_id
  , conLocalClipsId -- destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
