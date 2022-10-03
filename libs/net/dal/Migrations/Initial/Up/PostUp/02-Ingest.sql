DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
DECLARE syndicationId INT := (SELECT id FROM public.media_type WHERE Name = 'Syndication'); -- media_type_id
DECLARE videoId INT := (SELECT id FROM public.media_type WHERE Name = 'Video'); -- media_type_id
DECLARE audioId INT := (SELECT id FROM public.media_type WHERE Name = 'Audio'); -- media_type_id
DECLARE paperId INT := (SELECT id FROM public.media_type WHERE Name = 'Paper'); -- media_type_id

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
  , paperId -- media_type_id
  , (SELECT id FROM public.source WHERE code = 'GLOBE') -- source_id
  , 'GLOBE' -- topic
  , weeklyPrintId -- product_id
  , '{ "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true,
      "papername":"pubdata!name",
      "headline":"hl1",
      "summary":"hl1",
      "story":"body.content",
      "author":"byline",
      "date":"pubdata!date.publication",
      "id":"pubdata!id",
      "section":"pubdata!position.section",
      "page":"pubdata!position.sequence",
      "item":"nitf","dateFmt":"yyyyMMdd",
      "selfPublished": true,
      "dateOffset": -1 }' -- configuration
  , 0 -- schedule_type
  , 3 -- retry_limit
  , 5 -- source_connection_id
  , 6 -- destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'StarMetro'
  , '' -- description
  , true -- is_enabled
  , paperId -- media_type_id
  , (SELECT id FROM public.source WHERE code = 'STARMETRO') -- source_id
  , 'STARMETRO' -- topic
  , weeklyPrintId -- product_id
  , '{ "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true,
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
      "escape-content": false,
      "add-parent": false,
      "selfPublished": true,
      "dateOffset": -1 }' -- configuration
  , 0 -- schedule_type
  , 3 -- retry_limit
  , 5 -- source_connection_id
  , 6 -- destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Blacks Newsgroup'
  , '' -- description
  , true -- is_enabled
  , paperId -- media_type_id
  , (SELECT id FROM public.source WHERE code = 'BCNG') -- source_id
  , 'BCNG' -- topic
  , weeklyPrintId -- product_id
  , '{ "timeZone":"Pacific Standard Time",
      "language":"en-CA",
      "post":false,
      "import":false,
      "papername":"papername",
      "headline":"headline",
      "summary":"summary",
      "story":"story",
      "author":"author",
      "date":"date","id":"id",
      "item":"bcng",
      "page":"page",
      "section":"category",
      "dateFmt":"MM-dd-yyyy",
      "escapeContent":true,
      "addParent":true,
      "selfPublished":false,
      "filePattern":"^bcng-\u003Cdate\u003E-(.\u002B).xml",
      "dateOffset": -1,
      "sources":"Maple Ridge-Pitt Meadows News=MRN\u0026100 Mile House Free Press=100MILE\u0026Arrow Lakes News=ARROWLAKE\u0026Ashcroft Cache Creek Journal=ASHJOUR\u0026Barriere Star Journal=BARRSTARR\u0026Boundary Creek Times=BCT\u0026Burns Lake Lakes District News=BLLDN\u0026Caledonia Courier=CC\u0026Castlegar News=CN\u0026Clearwater Times=CT\u0026Coast Mountain News=CMN\u0026Cranbrook Townsman=CDT\u0026Creston Valley Advance=CVA\u0026Sicamouse Eagle Valley News=SEVN\u0026Fernie Free Press=TFP\u0026Golden Star=GS\u0026Grand Forks Gazette=GFG\u0026Houston Today=HT\u0026Invermere Valley Echo=IVE\u0026Kamloops This Week=KTW\u0026Kelowna Capital News=KCN\u0026Keremeos Review=KR\u0026Kimberley Bulletin=KDB\u0026Kitimat Northern Sentinel=KS\u0026Kootenay News Advertiser=KNA\u0026Lake Country Calendar=LCC\u0026Salmon Arm Lakeshore News=SALN\u0026Merritt Herald=MH\u0026Nelson Star=NS\u0026North Delta Reporter=NDR\u0026Prince Rupert Northern View=NV\u0026Penticton Western News=PW\u0026Prince George Free Press=PGFP\u0026Quesnel Cariboo Observer=QCO\u0026Revelstoke Review=RTR\u0026Rossland News=RN\u0026Salmon Arm Observer=SAO\u0026Similkameen Spotlight=SIMSP\u0026Smithers Interior News=SIN\u0026Summerland Review=SR\u0026Terrace Standard=TSTD\u0026Trail Daily Times=TDT\u0026Comox Valley Echo=CVE\u0026Vanderhoof Omineca Express=VOE\u0026Vernon Morning Star=VMS\u0026Williams Lake Tribune=WLT\u0026Abbotsford News=ABBNEWS\u0026Agassiz-Harrison Observer=AGASSIZ\u0026Aldergrove Star=ALDERSTAR\u0026Bowen Island Undercurrent=BIU\u0026Chilliwack Times=CTIMES\u0026Cloverdale Reporter=CRR\u0026Hope Standard=HS\u0026Langley Times=LT\u0026Langley Advance Times=LA\u0026Mission City Record=MCR\u0026North Shore Outlook=NSO\u0026Peace Arch News=PAN\u0026Richmond Review=RR\u0026Surrey Now-Leader=SURN\u0026Alberni Valley News=AVN\u0026Campbell River Mirror=CRM\u0026Comox Valley Record=CCVR\u0026Cowichan News Leader Pictorial=CNLP\u0026Cowichan Valley Citizen=CVC\u0026Goldstream News Gazette=GG\u0026Gulf Islands Driftwood=GID\u0026Ladysmith Chronicle=LC\u0026Lake Cowichan Gazette=LCG\u0026Monday Magazine=MM\u0026The Daily News (Nanaimo)=NANAIMO\u0026Nanaimo News Bulletin=NNB\u0026North Island Gazette=NIG\u0026Oak Bay News=OBN\u0026Parksville Qualicum Beach News=PQN\u0026Peninsula News Review=PNR\u0026Saanich News=SN\u0026Sooke News Mirror=SNM\u0026Tofino-Ucluelet Westerly News=TUWN\u0026Victoria News=VN\u0026Vancouver Island Free Daily=VIFD\u0026The Free Press=TFP\u0026Chemainus Valley Courier=CHVC\u0026Agassiz Observer=AGASSIZ\u0026Maple Ridge News=MRN\u0026Chilliwack Progress=CP\u0026The Northern View=NV\u0026Haida Gwaii Observer=HGO" }' -- configuration
  , 0 -- schedule_type
  , 3 -- retry_limit
  , 5 -- source_connection_id
  , 6 -- destination_connection_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Meltwater'
  , '' -- description
  , true -- is_enabled
  , paperId -- media_type_id
  , (SELECT id FROM public.source WHERE code = 'MELTWATER') -- source_id
  , 'MELTWATER' -- topic
  , weeklyPrintId -- product_id
  , '{ "timeZone":"Pacific Standard Time",
      "language":"en-CA",
      "post":false,
      "import":false,
      "papername":"!@PAPER=",
      "headline":"!@HEAD=",
      "summary":"!@ABSTRACT=",
      "story":"!@TEXT=",
      "author":"!@BYLINE=",
      "date":"!@DATE=",
      "lang":"!@LANG=",
      "section":"!@SECTION=",
      "id":"!@IDNUMBER=",
      "tags":"!@LKW=",
      "page":"!@PAGE=",
      "item":"**START-IO-STORY**",
      "dateFmt":"yyyyMMdd",
      "fileFormat":"fms",
      "filePattern":"^(.\u002B)\u003Cdate\u003E(.\u002B).fms$",
      "dateOffset": -1,
      "sources":"Vancouver Sun=SUN\u0026The Province=PROVINCE\u0026Times Colonist (Victoria)=TC\u0026National Post=POST\u0026Kelowna Daily Courier=KELOWNA\u0026Delta Optimist=DO\u0026North Shore News=NSN\u0026Burnaby Now=BNOW\u0026New West Record=NWR\u0026Richmond News=RNEWS\u0026Alaska Highway News=AHN\u0026Squamish Chief=SC\u0026Merritt Herald=MH\u0026Tri-City News=TCN\u0026Coast Reporter=CORE\u0026Dawson Creek Mirror=DCMR\u0026Kamloops This Week=KTW\u0026Peachland View=PV\u0026Prince George Citizen=PGC\u0026Oliver Chronicle=APOC" }' -- configuration
  , 0 -- schedule_type
  , 3 -- retry_limit
  , 5 -- source_connection_id
  , 6 -- destination_connection_id
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
