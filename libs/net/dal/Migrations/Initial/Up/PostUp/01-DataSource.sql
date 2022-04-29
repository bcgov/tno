DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.data_source (
  "name"
  , "code"
  , "description"
  , "is_enabled"
  , "media_type_id"
  , "data_location_id"
  , "license_id"
  , "topic"
  , "connection"
  , "parent_id"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Daily Hive'
  , 'DAILYHIVE'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{ "url":"http://dailyhive.com/feed/vancouver" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJCN'
  , 'CJCN'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Georgia Straight'
  , 'GEORGIA STRAIGHT'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{ "url":"http://www.straight.com/xml/feeds/bcg/news" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Burnaby Now'
  , 'BNOW'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Chilliwack Times'
  , 'CTIMES'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Tri-Cities Now'
  , 'TCNOW'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Record (New Westminster)'
  , 'NWR'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Richmond News'
  , 'RNEWS'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Castanet'
  , 'CASTANET'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{ "url":"https://www.castanet.net/rss/topheadlines.xml" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Coast Mountain News'
  , 'CMN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Salmon Arm Lakeshore News'
  , 'SALN'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Indo-Canadian Voice'
  , 'ICV'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Macleans'
  , 'MACL'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'iPolitics'
  , 'IPOLY'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{ "url":"http://www.ipolitics.ca/custom-feeds/bc-gov-feed.php" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Cranbrook Townsman'
  , 'CDT'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Delta Optimist'
  , 'DO'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Langley Advance Times'
  , 'LA'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Nelson Star'
  , 'NS'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Shore News'
  , 'NSN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Rossland News'
  , 'RN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Surrey Now-Leader'
  , 'SURN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Royal City Record'
  , 'RCR'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Announcement'
  , 'ANNOUNCE'
  , ''
  , true -- is_enabled
  , 13 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Media Availability'
  , 'MEDAV'
  , ''
  , true -- is_enabled
  , 13 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Scrum'
  , 'SCRUM'
  , ''
  , true -- is_enabled
  , 13 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Speech'
  , 'SPEECH'
  , ''
  , true -- is_enabled
  , 13 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Cloverdale Reporter'
  , 'CRR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Province'
  , 'PROVINCE'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Creston Valley Advance'
  , 'CVA'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Hook'
  , 'HOOK'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHKG'
  , 'CHKG'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CTV Online'
  , 'CTV ONLINE'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Canadian Centre for Policy Alternatives'
  , 'CCPA'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Cowichan Valley Citizen'
  , 'CVC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Haida Gwaii Observer'
  , 'HGO'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Delta Reporter'
  , 'NDR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Tofino-Ucluelet Westerly News'
  , 'TUWN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BNN'
  , 'BNN'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Trail Daily Times'
  , 'TDT'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Comox Valley Echo'
  , 'CVE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Abbotsford Times'
  , 'AT'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Edmonton Journal'
  , 'EJ'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Calgary Herald'
  , 'CH'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Oliver Chronicle'
  , 'APOC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'South Okanagan Times Chronicle'
  , 'TTC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Coast Reporter'
  , 'CORE'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Dawson Creek Mirror'
  , 'DCMR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Columbia Valley Pioneer'
  , 'CVP'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Chemainus Valley Courier'
  , 'CHVC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Opinion 250'
  , 'O250'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Fernie Free Press'
  , 'TFP'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Keremeos Review'
  , 'KR'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFTV'
  , 'CFTV'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJVB'
  , 'CJVB'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ming Pao News'
  , 'MING PAO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Sing Tao Daily'
  , 'SING TAO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver is Awesome'
  , 'VIAWE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'KRPI'
  , 'KRPI'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Calgary Herald (Print Edition)'
  , 'CHPE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Business in Vancouver'
  , 'BIV'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{ "url":"http://biv.com/rss" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHBC'
  , 'CHBC'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFJC'
  , 'CFJC'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHNL'
  , 'CHNL'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFNR'
  , 'CFNR'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKYE'
  , 'CKYE'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Bowen Island Undercurrent'
  , 'BIU'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Burns Lake Lakes District News'
  , 'BLLDN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Chilliwack Progress'
  , 'CP'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kelowna Capital News'
  , 'KCN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Monday Magazine'
  , 'MM'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Island Gazette'
  , 'NIG'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Sicamouse Eagle Valley News'
  , 'SEVN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Similkameen Spotlight'
  , 'SIMSP'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Daily News (Prince Rupert)'
  , 'PRDN'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CTV'
  , 'CTV'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Castlegar News'
  , 'CN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Hope Standard'
  , 'HS'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Island MidWeek'
  , 'NIMW'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Prince Rupert Northern View'
  , 'NV'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Alberni Valley News'
  , 'AVN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Valley Sentinel'
  , 'VS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Trail-Rossland News'
  , 'TRN'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver Courier'
  , 'VC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kootenay Western Star'
  , 'KWS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver Sun'
  , 'SUN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Times Colonist (Victoria)'
  , 'TC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFAX'
  , 'CFAX'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'AV Archive'
  , 'ARCHIVE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Regional'
  , 'REGIONAL'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKNW'
  , 'CKNW'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHEK'
  , 'CHEK'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHAN'
  , 'CHAN'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC'
  , 'CBC'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CIVT'
  , 'CIVT'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Globe and Mail'
  , 'GLOBE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{ "url":"sftp://gamdelivery.globeandmail.ca/", "username":"", "password":"" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Today''s Edition'
  , 'TE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CC News'
  , 'CCNEWS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Daily News (Kamloops)'
  , 'KAMLOOPS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKPG'
  , 'CKPG'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'SHAW'
  , 'SHAW'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'National Post'
  , 'POST'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CIVI'
  , 'CIVI'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Prince George Citizen'
  , 'PGC'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{ "url":"http://library.pressdisplay.com/test/qa/Services/AdvancedSearchRssHandler.ashx?srchText=%2a&srchnewspaper=7254&extended=false" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Open Cabinet'
  , 'OPENCABINET'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Daily Courier (Kelowna)'
  , 'KELOWNA'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKWX'
  , 'CKWX'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Press Theatre'
  , 'PRESS THEATRE'
  , ''
  , true -- is_enabled
  , 13 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  '100 Mile House Free Press'
  , '100MILE'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Abbotsford News'
  , 'ABBNEWS'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Agassiz-Harrison Observer'
  , 'AGASSIZ'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Aldergrove Star'
  , 'ALDERSTAR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Arrow Lakes News'
  , 'ARROWLAKE'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ashcroft Cache Creek Journal'
  , 'ASHJOUR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Barriere Star Journal'
  , 'BARRSTARR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Burnaby NewsLeader'
  , 'BNL'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Campbell River Mirror'
  , 'CRM'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Clearwater Times'
  , 'CT'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Comox Valley Record'
  , 'CCVR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Cowichan News Leader Pictorial'
  , 'CNLP'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Esquimalt News'
  , 'EN'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Fort Saint James Courier'
  , 'FSJC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Golden Star'
  , 'GS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Goldstream News Gazette'
  , 'GG'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Houston Today'
  , 'HT'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Invermere Valley Echo'
  , 'IVE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kamloops This Week'
  , 'KTW'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kitimat Northern Sentinel'
  , 'KS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kootenay News Advertiser'
  , 'KNA'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ladysmith Chronicle'
  , 'LC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Lake Cowichan Gazette'
  , 'LCG'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Lakes District News'
  , 'LDN'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Langley Times'
  , 'LT'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Maple Ridge-Pitt Meadows News'
  , 'MRN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Merritt Herald'
  , 'MH'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Mission City Record'
  , 'MCR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Nanaimo News Bulletin'
  , 'NNB'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'New Westminster News Leader'
  , 'NWNL'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Island Weekender'
  , 'NIW'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Shore Outlook'
  , 'NSO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Oak Bay News'
  , 'OBN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Parksville Qualicum Beach News'
  , 'PQN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Peace Arch News'
  , 'PAN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Peninsula News Review'
  , 'PNR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Penticton Western News'
  , 'PW'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Port Hardy North Island Gazette'
  , 'PHNIG'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Prince George Free Press'
  , 'PGFP'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Princeton Similkameen Spotlight'
  , 'PSS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Government and Service Employees'' Union'
  , 'BCGEU'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'New Westminster Record'
  , 'NWREC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBYK'
  , 'CBYK'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CorpCal'
  , 'CorpCal'
  , ''
  , true -- is_enabled
  , 16 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'iNFOnews'
  , 'INFONEWS'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Link'
  , 'LINK'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Quesnel Cariboo Observer'
  , 'QCO'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Revelstoke Review'
  , 'RTR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Richmond Review'
  , 'RR'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Saanich News'
  , 'SN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Salmon Arm Observer'
  , 'SAO'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Smithers Interior News'
  , 'SIN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Sooke News Mirror'
  , 'SNM'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'South Delta Leader'
  , 'SDL'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Summerland Review'
  , 'SR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Surrey North Delta Leader'
  , 'SL'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Terrace Standard'
  , 'TSTD'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Chilliwack Progress'
  , 'TCP'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Tri-City News'
  , 'TCN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vanderhoof Omineca Express'
  , 'VOE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vernon Morning Star'
  , 'VMS'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Victoria News'
  , 'VN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Victoria Weekend'
  , 'VW'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'WestEnder'
  , 'WESTENDER'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Williams Lake Tribune'
  , 'WLT'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC Online'
  , 'CBCO'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{ "url":"https://www.cbc.ca/cmlink/rss-topstories" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Daily News (Nanaimo)'
  , 'NANAIMO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Lake Country Calendar'
  , 'LCC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Caledonia Courier'
  , 'CC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKFR'
  , 'CKFR'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Nelson Daily News'
  , 'NDN'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CP News'
  , 'CPNEWS'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{ "url":"http://www.commandnews.com/fpweb/fp.dll/$bc-rss/htm/rss/x_searchlist.htm/_drawerid/!default_bc-rss/_profileid/rss/_iby/daj/_iby/daj/_svc/cp_pub/_k/XQkKHjnAUpumRfdr" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Gulf Islands Driftwood'
  , 'GID'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFIS'
  , 'CFIS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBCV'
  , 'CBCV'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBTK'
  , 'CBTK'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBU'
  , 'CBU'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBYG'
  , 'CBYG'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBUT'
  , 'CBUT'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The New York Times'
  , 'NYT'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Daily Telegraph'
  , 'DTL'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKNW Online'
  , 'CKNW ONLINE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Montreal Gazette'
  , 'MG'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Toronto Star'
  , 'TS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKWX Online'
  , 'CKWX ONLINE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Seattle PI Online'
  , 'SPIO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Georgia Straight Online'
  , 'GSO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver Province Online'
  , 'PROVO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Globe and Mail Online'
  , 'GMO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Victoria Buzz'
  , 'VBUZZ'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{ "url":"http://www.victoriabuzz.com/feed/" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'KXLY'
  , 'KXLY'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Global News: BC 1'
  , 'BC 1'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Boundary Creek Times'
  , 'BCT'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kimberley Bulletin'
  , 'KDB'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Peachland View'
  , 'PV'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Conservative Party'
  , 'BC CONS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'News Kamloops'
  , 'NEWKAM'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Sierra Club of BC'
  , 'SIERRA'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Webcast'
  , 'WEBCAST'
  , ''
  , true -- is_enabled
  , 14 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Squamish Chief'
  , 'SC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver Sun Online'
  , 'SUN ONLINE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'HuffPostBC'
  , 'HUFFPOSTBC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJVB Online'
  , 'CJVB ONLINE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKSP'
  , 'CKSP'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Orca'
  , 'ORCA'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{ "url":"https://theorca.ca/feed/" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKVU'
  , 'CKVU'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBX'
  , 'CBX'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kelowna Westside Weekly'
  , 'KWW'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKSP Sameer Kaushal'
  , 'CKSP SAMEER'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Alaska Highway News'
  , 'AHN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Northerner'
  , 'FSJN'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKFU'
  , 'CKFU'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver 24 hrs'
  , '24HRS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'StarMetro'
  , 'STARMETRO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Union of BC Indian Chiefs'
  , 'UBCIC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'TVS - Talentvision'
  , 'TVS'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CityCaucus.com'
  , 'CITYC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Hospital Employees'' Union'
  , 'HEU'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Health Coalition'
  , 'BCHC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHMB'
  , 'CHMB'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJRJ'
  , 'CJRJ'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'KVRI'
  , 'KVRI'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'British Columbia Nurses'' Union'
  , 'BCNU'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'PTC'
  , 'PTC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver Island Free Daily'
  , 'VIFD'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Tyee'
  , 'TYEE'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Canadian Union of Public Employees'
  , 'CUPE BC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'APTN'
  , 'APTN'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Social Media'
  , 'BCPOLI'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKPG Online'
  , 'CKPG ONLINE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHNM'
  , 'CHNM'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'British Columbia Federation of Labour'
  , 'BC FED'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Grand Forks Gazette'
  , 'GFG'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Teachers'' Federation'
  , 'BCTF'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Public Eye Online'
  , 'PEO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Local News'
  , 'BCLN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBCIndigNews'
  , 'CBCINDIGNEWS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBCBCNews'
  , 'CBCBCNEWS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Narwhal'
  , 'NAR'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{ "url":"https://thenarwhal.ca/feed/rss2" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Infotel'
  , 'INFOTEL'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , ''
  , '{ "url":"https://infotel.ca/govbcrssfeed" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
