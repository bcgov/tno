DO $$
DECLARE ingestSyndicationId INT := (SELECT id FROM public.ingest_type WHERE "name" = 'Syndication'); -- ingest_type_id
DECLARE ingestVideoId INT := (SELECT id FROM public.ingest_type WHERE "name" = 'Video'); -- ingest_type_id
DECLARE ingestAudioId INT := (SELECT id FROM public.ingest_type WHERE "name" = 'Audio'); -- ingest_type_id
DECLARE ingestPaperId INT := (SELECT id FROM public.ingest_type WHERE "name" = 'Paper'); -- ingest_type_id
DECLARE ingestFrontPageId INT := (SELECT id FROM public.ingest_type WHERE "name" = 'Front Page'); -- ingest_type_id

DECLARE wireId INT := (SELECT id FROM public.product WHERE "name" = 'CP Wire'); -- product_id
DECLARE frontPageId INT := (SELECT id FROM public.product WHERE "name" = 'Front Page'); -- product_id
DECLARE talkRadioId INT := (SELECT id FROM public.product WHERE "name" = 'Talk Radio'); -- product_id
DECLARE videoNewsId INT := (SELECT id FROM public.product WHERE "name" = 'TV / Video News'); -- product_id
DECLARE weeklyPrintId INT := (SELECT id FROM public.product WHERE "name" = 'Weekly Print'); -- product_id
DECLARE dailyPrintId INT := (SELECT id FROM public.product WHERE "name" = 'Daily Print'); -- product_id
DECLARE onlinePrintId INT := (SELECT id FROM public.product WHERE "name" = 'Online Print'); -- product_id

DECLARE conNoneId INT := (SELECT id FROM public.connection WHERE "name" = 'None'); -- connection_id
DECLARE conLocalStreamsId INT := (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Streams'); -- connection_id
DECLARE conLocalClipsId INT := (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Clips'); -- connection_id
DECLARE conLocalImagesId INT := (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Images'); -- connection_id
DECLARE conLocalPapersId INT := (SELECT id FROM public.connection WHERE "name" = 'Local Volume - Papers'); -- connection_id
DECLARE conPublicInternetId INT := (SELECT id FROM public.connection WHERE "name" = 'Public Internet'); -- connection_id
DECLARE conSSHId INT := (SELECT id FROM public.connection WHERE "name" = 'SSH - Newspaper Upload'); -- connection_id
DECLARE conGlobeSSHId INT := (SELECT id FROM public.connection WHERE "name" = 'SSH - Globe Newspaper Upload'); -- connection_id
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
-- ******************************************************
-- Syndication
-- ******************************************************
(
  'Daily Hive'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
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
  , ''
  , ''
), (
  'The Georgia Straight'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
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
  , ''
  , ''
), (
  'Castanet'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
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
  , ''
  , ''
), (
  'iPolitics'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'IPOLY') -- source_id
  , 'IPOLY' -- topic
  , wireId -- product_id
  , '{ "url":"https://old.ipolitics.ca/custom-feeds/bc-gov-feed.php",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , ''
  , ''
), (
  'Business in Vancouver'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
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
  , ''
  , ''
), (
  'Prince George Citizen'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
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
  , ''
  , ''
), (
  'CBC Online'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
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
  , ''
  , ''
), (
  'Canadian Press Wire'
  , ''
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CPNEWS') -- source_id
  , 'CPNEWS' -- topic
  , wireId -- product_id
  , '{ "url":"http://www.commandnews.com/fpweb/fp.dll/$bc-rss/htm/rss/x_searchlist.htm/_drawerid/!default_bc-rss/_profileid/rss/_iby/daj/_iby/daj/_svc/cp_pub/_k/XQkKHjnAUpumRfdr",
      "timeZone": "Eastern Standard Time",
      "language": "en-CA",
      "fetchContent": true,
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , ''
  , ''
), (
  'Victoria Buzz'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
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
  , ''
  , ''
), (
  'Orca'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
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
  , ''
  , ''
), (
  'Narwhal'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
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
  , ''
  , ''
), (
  'iNFOnews'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
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
  , ''
  , ''
), (
  'Ha-Shilth-Sa'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'HaShilthSa') -- source_id
  , 'Ha-Shilth-Sa' -- topic
  , wireId -- product_id
  , '{ "import": true,
      "post": true,
      "timeZone": "Pacific Standard Time",
      "url": "https://hashilthsa.com/news/" }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , ''
  , ''
), (
  'The Tyee'
  , '' -- description
  , true -- is_enabled
  , ingestSyndicationId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'TYEE') -- source_id
  , 'TYEE' -- topic
  , onlinePrintId -- product_id
  , '{ "import": true,
      "post": true,
      "url": "https://thetyee.ca/News/",
      "timeZone": "Pacific Standard Time" }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conNoneId--destination_connection_id
  , ''
  , ''
),

-- ******************************************************
-- Paper
-- ******************************************************
(
  'Globe & Mail - Articles'
  , '' -- description
  , true -- is_enabled
  , ingestPaperId -- media_type_id
  , (SELECT id FROM public.source WHERE code = 'GLOBE') -- source_id
  , 'GLOBE' -- topic
  , dailyPrintId -- product_id
  , '{ "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true,
      "path": "",
      "papername": "pubdata!name",
      "headline": "hl1",
      "summary": "hl1",
      "story": "body.content",
      "author": "byline",
      "date": "pubdata!date.publication",
      "id": "pubdata!id",
      "section": "pubdata!position.section",
      "page": "pubdata!position.sequence",
      "item": "nitf",
      "dateFmt": "yyyyMMdd",
      "filePattern":"^<date>(.+).xml$",
      "dateOffset": 0 }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conGlobeSSHId -- source_connection_id
  , conLocalPapersId -- destination_connection_id
  , ''
  , ''
), (
  'StarMetro'
  , '' -- description
  , false -- is_enabled
  , ingestPaperId -- media_type_id
  , (SELECT id FROM public.source WHERE code = 'STARMETRO') -- source_id
  , 'STARMETRO' -- topic
  , weeklyPrintId -- product_id
  , '{ "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true,
      "path": "processed",
      "papername": "pubdata!name",
      "headline": "hl1",
      "summary": "hl1",
      "story": "body.content",
      "author": "byline",
      "date": "pubdata!date.publication",
      "id": "doc-id!id-string",
      "section": "pubdata!position.section",
      "page": "pubdata!position.sequence",
      "item": "nitf",
      "dateFmt": "yyyyMMdd",
      "escapeContent": false,
      "addParent": false,
      "dateOffset": 0 }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conSSHId -- source_connection_id
  , conLocalPapersId -- destination_connection_id
  , ''
  , ''
), (
  'Blacks Newsgroup'
  , '' -- description
  , true -- is_enabled
  , ingestPaperId -- media_type_id
  , (SELECT id FROM public.source WHERE code = 'BCNG') -- source_id
  , 'BCNG' -- topic
  , weeklyPrintId -- product_id
  , '{ "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true,
      "path": "processed",
      "papername": "papername",
      "headline": "headline",
      "summary": "summary",
      "story": "story",
      "author": "author",
      "date": "date",
      "id":"id",
      "item": "bcng",
      "page": "page",
      "section": "category",
      "dateFmt": "MM-dd-yyyy",
      "escapeContent": true,
      "addParent": true,
      "filePattern":" ^bcng-<date>-(.+).xml$",
      "dateOffset": -1,
      "sources": "Maple Ridge-Pitt Meadows News=MRN&100 Mile House Free Press=100MILE&Arrow Lakes News=ARROWLAKE&Ashcroft Cache Creek Journal=ASHJOUR&Barriere Star Journal=BARRSTARR&Boundary Creek Times=BCT&Burns Lake Lakes District News=BLLDN&Caledonia Courier=CC&Castlegar News=CN&Clearwater Times=CT&Coast Mountain News=CMN&Cranbrook Townsman=CDT&Creston Valley Advance=CVA&Sicamous Eagle Valley News=SEVN&Fernie Free Press=TFP&Golden Star=GS&Grand Forks Gazette=GFG&Houston Today=HT&Invermere Valley Echo=IVE&Kamloops This Week=KTW&Kelowna Capital News=KCN&Keremeos Review=KR&Kimberley Bulletin=KDB&Kitimat Northern Sentinel=KS&Kootenay News Advertiser=KNA&Lake Country Calendar=LCC&Salmon Arm Lakeshore News=SALN&Merritt Herald=MH&Nelson Star=NS&North Delta Reporter=NDR&Prince Rupert Northern View=NV&Penticton Western News=PW&Prince George Free Press=PGFP&Quesnel Cariboo Observer=QCO&Revelstoke Review=RTR&Rossland News=RN&Salmon Arm Observer=SAO&Similkameen Spotlight=SIMSP&Smithers Interior News=SIN&Summerland Review=SR&Terrace Standard=TSTD&Trail Daily Times=TDT&Comox Valley Echo=CVE&Vanderhoof Omineca Express=VOE&Vernon Morning Star=VMS&Williams Lake Tribune=WLT&Abbotsford News=ABBNEWS&Agassiz-Harrison Observer=AGASSIZ&Aldergrove Star=ALDERSTAR&Bowen Island Undercurrent=BIU&Chilliwack Times=CTIMES&Cloverdale Reporter=CRR&Hope Standard=HS&Langley Times=LT&Langley Advance Times=LA&Mission City Record=MCR&North Shore Outlook=NSO&Peace Arch News=PAN&Richmond Review=RR&Surrey Now-Leader=SURN&Alberni Valley News=AVN&Campbell River Mirror=CRM&Comox Valley Record=CCVR&Cowichan News Leader Pictorial=CNLP&Cowichan Valley Citizen=CVC&Goldstream News Gazette=GG&Gulf Islands Driftwood=GID&Ladysmith Chronicle=LC&Lake Cowichan Gazette=LCG&Monday Magazine=MM&The Daily News (Nanaimo)=NANAIMO&Nanaimo News Bulletin=NNB&North Island Gazette=NIG&Oak Bay News=OBN&Parksville Qualicum Beach News=PQN&Peninsula News Review=PNR&Saanich News=SN&Sooke News Mirror=SNM&Tofino-Ucluelet Westerly News=TUWN&Victoria News=VN&Vancouver Island Free Daily=VIFD&The Free Press=TFP&Chemainus Valley Courier=CHVC&Agassiz Observer=AGASSIZ&Maple Ridge News=MRN&Chilliwack Progress=CP&The Northern View=NV&Haida Gwaii Observer=HGO" }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conSSHId -- source_connection_id
  , conLocalPapersId -- destination_connection_id
  , ''
  , ''
), (
  'Meltwater'
  , '' -- description
  , true -- is_enabled
  , ingestPaperId -- media_type_id
  , (SELECT id FROM public.source WHERE code = 'MELTWATER') -- source_id
  , 'MELTWATER' -- topic
  , weeklyPrintId -- product_id
  , '{ "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true,
      "path": "processed",
      "papername": "!@PAPER=",
      "headline": "!@HEAD=",
      "summary": "!@ABSTRACT=",
      "story": "!@TEXT=",
      "author": "!@BYLINE=",
      "date": "!@DATE=",
      "lang": "!@LANG=",
      "section": "!@SECTION=",
      "id": "!@IDNUMBER=",
      "tags": "!@LKW=",
      "page": "!@PAGE=",
      "item": "**START-IO-STORY**",
      "dateFmt": "yyyyMMdd",
      "fileFormat": "fms",
      "filePattern": "^(.+)<date>(.+).fms$",
      "dateOffset": 0,
      "sources": "Vancouver Sun=SUN&The Province=PROVINCE&Times Colonist (Victoria)=TC&National Post=POST&Kelowna Daily Courier=KELOWNA&Delta Optimist=DO&North Shore News=NSN&Burnaby Now=BNOW&New West Record=NWR&Richmond News=RNEWS&Alaska Highway News=AHN&Squamish Chief=SC&Merritt Herald=MH&Tri-City News=TCN&Coast Reporter=CORE&Dawson Creek Mirror=DCMR&Kamloops This Week=KTW&Peachland View=PV&Prince George Citizen=PGC&Oliver Chronicle=APOC" }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conSSHId -- source_connection_id
  , conLocalPapersId -- destination_connection_id
  , ''
  , ''
),

-- ******************************************************
-- Front Page
-- ******************************************************
(
  'Globe & Mail - Page 1'
  , 'Globe and Mail newspaper frontpage images' -- description
  , true -- is_enabled
  , ingestFrontPageId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'GLOBE') -- source_id
  , 'GLOBE' -- topic
  , frontPageId -- product_id
  , '{ "path": "binaryroot",
      "fileName": "sv-GLB",
      "publishedOnExpression": "^sv-GLB-[A-Za-z]{3}-(?<day>[0-9]{2})(?<month>[0-9]{2})(?<year>[0-9]{4})",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conSSHId --destination_connection_id
  , conLocalImagesId -- destination_connection_id
  , ''
  , ''
), (
  'National Post - Page 1'
  , 'National Post newspaper frontpage images' -- description
  , true -- is_enabled
  , ingestFrontPageId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'POST') -- source_id
  , 'POST' -- topic
  , frontPageId -- product_id
  , '{ "path": "binaryroot",
      "fileName": "NTNP",
      "publishedOnExpression": "^NTNP(?<year>[0-9]{4})(?<month>[0-9]{2})(?<day>[0-9]{2})(?<hour>[0-9]{2})(?<minute>[0-9]{2})(?<second>[0-9]{2})",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conSSHId --destination_connection_id
  , conLocalImagesId -- destination_connection_id
  , ''
  , ''
), (
  'The Province - Page 1'
  , 'The Province newspaper frontpage images' -- description
  , true -- is_enabled
  , ingestFrontPageId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'PROVINCE') -- source_id
  , 'PROVINCE' -- topic
  , frontPageId -- product_id
  , '{ "path": "binaryroot",
      "fileName": "VAPR",
      "publishedOnExpression": "^VAPR(?<year>[0-9]{4})(?<month>[0-9]{2})(?<day>[0-9]{2})(?<hour>[0-9]{2})(?<minute>[0-9]{2})(?<second>[0-9]{2})",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conSSHId --destination_connection_id
  , conLocalImagesId -- destination_connection_id
  , ''
  , ''
), (
  'Times Colonist Victoria - Page 1'
  , 'Times Colonist Victoria newspaper frontpage images' -- description
  , true -- is_enabled
  , ingestFrontPageId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'TC') -- source_id
  , 'TC' -- topic
  , frontPageId -- product_id
  , '{ "path": "binaryroot",
      "fileName": "VITC",
      "publishedOnExpression": "^VITC(?<year>[0-9]{4})(?<month>[0-9]{2})(?<day>[0-9]{2})(?<hour>[0-9]{2})(?<minute>[0-9]{2})(?<second>[0-9]{2})",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conSSHId --destination_connection_id
  , conLocalImagesId -- destination_connection_id
  , ''
  , ''
), (
  'Vancouver Sun - Page 1'
  , 'Vancouver Sun newspaper frontpage images' -- description
  , true -- is_enabled
  , ingestFrontPageId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'SUN') -- source_id
  , 'SUN' -- topic
  , frontPageId -- product_id
  , '{ "path": "binaryroot",
      "fileName": "VASN",
      "publishedOnExpression": "^VASN(?<year>[0-9]{4})(?<month>[0-9]{2})(?<day>[0-9]{2})(?<hour>[0-9]{2})(?<minute>[0-9]{2})(?<second>[0-9]{2})",
      "post": true,
      "import": true }' -- configuration
  , 1 -- schedule_type
  , 3 -- retry_limit
  , conSSHId --destination_connection_id
  , conLocalImagesId -- destination_connection_id
  , ''
  , ''
),

-- ******************************************************
-- Video
-- ******************************************************
(
  'CBC Newsworld'
  , 'Stay on top of British Columbia with the latest in news, weather, sports and interviews.' -- description
  , true -- is_enabled
  , ingestVideoId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBC') -- source_id
  , 'CBC' -- topic
  , videoNewsId -- product_id
  , '{ "serviceType": "stream",
      "url": "https://cbcrclinear-tor.akamaized.net/hls/live/2042769/geo_allow_ca/CBCRCLINEAR_TOR_15/master4.m3u8",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "fileName": "{schedule.Name}.mpg",
      "post": true,
      "import": true }' -- configuration
  , 3 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalClipsId -- destination_connection_id
  , ''
  , ''
), (
  'Raspberry Pi4'
  , '' -- description
  , true -- is_enabled
  , ingestVideoId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBC') -- source_id
  , 'CBC' -- topic
  , videoNewsId -- product_id
  , '{ "serviceType": "RPi",
      "import": false,
      "post": false,
      "hostname": "TNOMediaPi4",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "logLevel": "verbose",
      "videoInput": "/dev/video0",
      "videoThreadQueueSize": "1024",
      "videoInputFormat": "v4l2",
      "videoFramerate": "60",
      "audioInput": "hw:CARD=HDMI",
      "audioThreadQueueSize": "1024",
      "audioInputFormat": "alsa",
      "audioChannels": "2",
      "audioChannelLayout": "stereo",
      "bufferSize": "",
      "audioEncoder": "",
      "audioBufferSize": "",
      "videoEncoder": "",
      "videoBufferSize": "",
      "pixelFormat": "",
      "keyframe": "",
      "preset": "",
      "crf": "",
      "otherArgs": "",
      "fileName": "{schedule.Name}.mpg",
      "maxRate": "" }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conNoneId -- source_connection_id
  , conLocalStreamsId -- destination_connection_id
  , ''
  , ''
), (
  'Raspberry Pi5'
  , '' -- description
  , true -- is_enabled
  , ingestVideoId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBC') -- source_id
  , 'CBC' -- topic
  , videoNewsId -- product_id
  , '{ "serviceType": "RPi",
      "import": false,
      "post": false,
      "hostname": "TNOMediaPi5",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "logLevel": "verbose",
      "videoInput": "/dev/video0",
      "videoThreadQueueSize": 1024,
      "videoInputFormat": "v4l2",
      "videoFramerate": "60",
      "audioInput": "hw:CARD=HDMI",
      "audioThreadQueueSize": "1024",
      "audioInputFormat": "alsa",
      "audioChannels": "2",
      "audioChannelLayout": "stereo",
      "bufferSize": "",
      "maxRate": "",
      "fileName": "{schedule.Name}.mpg" }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conNoneId -- source_connection_id
  , conLocalStreamsId -- destination_connection_id
  , ''
  , ''
),

-- ******************************************************
-- Audio
-- ******************************************************
(
  'CBC Victoria - Stream' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
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
  , '' -- created_by
  , '' -- updated_by
), (
  'CBC Kamloops' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBC') -- source_id
  , 'CBC' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://cbcmp3.ic.llnwd.net/stream/cbcmp3_cbc_r1_kam",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CBC Kelowna' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBC') -- source_id
  , 'CBC' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://cbcmp3.ic.llnwd.net/stream/cbcmp3_cbc_r1_kel",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CBC Prince George' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBC') -- source_id
  , 'CBC' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://cbcmp3.ic.llnwd.net/stream/cbcmp3_cbc_r1_prg",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CBC Vancouver' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBC') -- source_id
  , 'CBC' -- topic
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
  , '' -- created_by
  , '' -- updated_by
), (
  'CBC Victoria' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBC') -- source_id
  , 'CBC' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://cbcmp3.ic.llnwd.net/stream/cbcmp3_cbc_r1_vic",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CHKG' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CHKG') -- source_id
  , 'CHKG' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://icecast01.eseenet.com:8000/fm961",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CHMB' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CHMB') -- source_id
  , 'CHMB' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://stream.radiojar.com/nugqupcv8qzuv",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CHNL' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CHNL') -- source_id
  , 'CHNL' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://newcap.leanstream.co/CHNLAM-MP3?args=3rdparty_01",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CJCN Connect FM' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CJCN') -- source_id
  , 'CJCN' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://n12.rcs.revma.com/ygxn9vgennruv",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CJVB' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CJVB') -- source_id
  , 'CJVB' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://icecast01.eseenet.com:8000/am1470",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CKFU' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CKFU') -- source_id
  , 'CKFU' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://18223.live.streamtheworld.com/MOOSEFM_SC",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CKSP' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CKSP') -- source_id
  , 'CKSP' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://ais-sa1.streamon.fm/7676_48k.aac",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CKWX' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CKWX') -- source_id
  , 'CKWX' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://rogers.leanstream.co/rogers/van1130.stream/icy",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CKYE' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CKYE') -- source_id
  , 'CKYE' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://ice10.securenetsystems.net/CKYE",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CKNW' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CKNW') -- source_id
  , 'CKNW' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://live.leanstream.co/CKNWAM-MP3",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CFAX' -- name
  , 'http://playerservices.streamtheworld.com/pls/CFAXAM.pls' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CFAX') -- source_id
  , 'CFAX' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://18393.live.streamtheworld.com:80/CFAXAM_SC",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'KNKX' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'KNKX') -- source_id
  , 'KNKX' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://34.83.21.216/ppm-knkxfmaac-ibc1?session-id=f40c97fb0b50d0bb74b7f0131f220aed",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CKFR' -- name
  , 'http://playerservices.streamtheworld.com/pls/CKFRAM.pls' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CKFR') -- source_id
  , 'CKFR' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://19813.live.streamtheworld.com:80/CKFRAM_SC",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CBC R2' -- name
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBC') -- source_id
  , 'CBC' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType":"stream",
      "url": "http://cbcmp3.ic.llnwd.net/stream/cbcmp3_cbc_r2_vcr",
      "timeZone":"Pacific Standard Time",
      "language": "en-CA",
      "post": false,
      "import": false }' -- configuration
  , 2 -- schedule_type
  , 3 -- retry_limit
  , conPublicInternetId --destination_connection_id
  , conLocalStreamsId -- destination_connection_id
  , '' -- created_by
  , '' -- updated_by
), (
  'CBC Victoria - Clips'
  , '' -- description
  , true -- is_enabled
  , ingestAudioId -- ingest_type_id
  , (SELECT id FROM public.source WHERE code = 'CBCV') -- source_id
  , 'CBCV' -- topic
  , talkRadioId -- product_id
  , '{ "serviceType": "clip",
      "keepChecking": true,
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true }' -- configuration
  , 3 -- schedule_type
  , 3 -- retry_limit
  , conLocalStreamsId -- source_connection_id
  , conLocalClipsId -- destination_connection_id
  , ''
  , ''
);

END $$;
