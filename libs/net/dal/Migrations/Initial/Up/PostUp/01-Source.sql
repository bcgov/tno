DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.source (
  "name"
  , "code"
  , "short_name"
  , "description"
  , "is_enabled"
  , "auto_transcribe"
  , "disable_transcribe"
  , "license_id"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Daily Hive'
  , 'DAILYHIVE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJCN'
  , 'CJCN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Georgia Straight'
  , 'GEORGIA STRAIGHT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Burnaby Now'
  , 'BNOW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Chilliwack Times'
  , 'CTIMES'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Tri-Cities Now'
  , 'TCNOW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Record (New Westminster)'
  , 'NWR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Richmond News'
  , 'RNEWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Castanet'
  , 'CASTANET'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Coast Mountain News'
  , 'CMN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Salmon Arm Lakeshore News'
  , 'SALN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Indo-Canadian Voice'
  , 'ICV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Macleans'
  , 'MACL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'iPolitics'
  , 'IPOLY'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Cranbrook Townsman'
  , 'CDT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Delta Optimist'
  , 'DO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Langley Advance Times'
  , 'LA'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Nelson Star'
  , 'NS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Shore News'
  , 'NSN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Rossland News'
  , 'RN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Surrey Now-Leader'
  , 'SURN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Royal City Record'
  , 'RCR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Announcement'
  , 'ANNOUNCE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Media Availability'
  , 'MEDAV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Scrum'
  , 'SCRUM'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Speech'
  , 'SPEECH'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Cloverdale Reporter'
  , 'CRR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Creston Valley Advance'
  , 'CVA'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Hook'
  , 'HOOK'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHKG'
  , 'CHKG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CTV Online'
  , 'CTV ONLINE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Canadian Centre for Policy Alternatives'
  , 'CCPA'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Cowichan Valley Citizen'
  , 'CVC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Haida Gwaii Observer'
  , 'HGO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Delta Reporter'
  , 'NDR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Tofino-Ucluelet Westerly News'
  , 'TUWN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BNN'
  , 'BNN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Trail Daily Times'
  , 'TDT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Comox Valley Echo'
  , 'CVE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Abbotsford Times'
  , 'AT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Edmonton Journal'
  , 'EJ'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Calgary Herald'
  , 'CH'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Oliver Chronicle'
  , 'APOC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'South Okanagan Times Chronicle'
  , 'TTC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Coast Reporter'
  , 'CORE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Dawson Creek Mirror'
  , 'DCMR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Columbia Valley Pioneer'
  , 'CVP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Chemainus Valley Courier'
  , 'CHVC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Opinion 250'
  , 'O250'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Fernie Free Press'
  , 'TFP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Keremeos Review'
  , 'KR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFTV'
  , 'CFTV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJVB'
  , 'CJVB'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ming Pao News'
  , 'MING PAO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Sing Tao Daily'
  , 'SING TAO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver is Awesome'
  , 'VIAWE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'KRPI'
  , 'KRPI'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Calgary Herald (Print Edition)'
  , 'CHPE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Business in Vancouver'
  , 'BIV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHBC'
  , 'CHBC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFJC'
  , 'CFJC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHNL'
  , 'CHNL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFNR'
  , 'CFNR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKYE'
  , 'CKYE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Bowen Island Undercurrent'
  , 'BIU'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Burns Lake Lakes District News'
  , 'BLLDN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Chilliwack Progress'
  , 'CP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kelowna Capital News'
  , 'KCN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Monday Magazine'
  , 'MM'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Island Gazette'
  , 'NIG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Sicamouse Eagle Valley News'
  , 'SEVN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Similkameen Spotlight'
  , 'SIMSP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Daily News (Prince Rupert)'
  , 'PRDN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CTV'
  , 'CTV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Castlegar News'
  , 'CN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Hope Standard'
  , 'HS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Island MidWeek'
  , 'NIMW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Prince Rupert Northern View'
  , 'NV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Alberni Valley News'
  , 'AVN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Valley Sentinel'
  , 'VS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Trail-Rossland News'
  , 'TRN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver Courier'
  , 'VC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kootenay Western Star'
  , 'KWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFAX'
  , 'CFAX'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'AV Archive'
  , 'ARCHIVE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Regional'
  , 'REGIONAL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKNW'
  , 'CKNW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHEK'
  , 'CHEK'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHAN'
  , 'CHAN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC'
  , 'CBC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CIVT'
  , 'CIVT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Globe and Mail'
  , 'GLOBE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Today''s Edition'
  , 'TE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CC News'
  , 'CCNEWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Daily News (Kamloops)'
  , 'KAMLOOPS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKPG'
  , 'CKPG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'SHAW'
  , 'SHAW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CIVI'
  , 'CIVI'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Prince George Citizen'
  , 'PGC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Open Cabinet'
  , 'OPENCABINET'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Daily Courier (Kelowna)'
  , 'KELOWNA'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKWX'
  , 'CKWX'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Press Theatre'
  , 'PRESS THEATRE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  '100 Mile House Free Press'
  , '100MILE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Abbotsford News'
  , 'ABBNEWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Agassiz-Harrison Observer'
  , 'AGASSIZ'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Aldergrove Star'
  , 'ALDERSTAR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Arrow Lakes News'
  , 'ARROWLAKE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ashcroft Cache Creek Journal'
  , 'ASHJOUR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Barriere Star Journal'
  , 'BARRSTARR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Burnaby NewsLeader'
  , 'BNL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Campbell River Mirror'
  , 'CRM'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Clearwater Times'
  , 'CT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Comox Valley Record'
  , 'CCVR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Cowichan News Leader Pictorial'
  , 'CNLP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Esquimalt News'
  , 'EN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Fort Saint James Courier'
  , 'FSJC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Golden Star'
  , 'GS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Goldstream News Gazette'
  , 'GG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Houston Today'
  , 'HT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Invermere Valley Echo'
  , 'IVE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kamloops This Week'
  , 'KTW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kitimat Northern Sentinel'
  , 'KS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kootenay News Advertiser'
  , 'KNA'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ladysmith Chronicle'
  , 'LC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Lake Cowichan Gazette'
  , 'LCG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Lakes District News'
  , 'LDN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Langley Times'
  , 'LT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Maple Ridge-Pitt Meadows News'
  , 'MRN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Merritt Herald'
  , 'MH'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Mission City Record'
  , 'MCR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Nanaimo News Bulletin'
  , 'NNB'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'New Westminster News Leader'
  , 'NWNL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Island Weekender'
  , 'NIW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Shore Outlook'
  , 'NSO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Oak Bay News'
  , 'OBN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Parksville Qualicum Beach News'
  , 'PQN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Peace Arch News'
  , 'PAN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Peninsula News Review'
  , 'PNR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Penticton Western News'
  , 'PW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Port Hardy North Island Gazette'
  , 'PHNIG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Prince George Free Press'
  , 'PGFP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Princeton Similkameen Spotlight'
  , 'PSS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Government and Service Employees'' Union'
  , 'BCGEU'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'New Westminster Record'
  , 'NWREC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBYK'
  , 'CBYK'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CorpCal'
  , 'CorpCal'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'iNFOnews'
  , 'INFONEWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Link'
  , 'LINK'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Quesnel Cariboo Observer'
  , 'QCO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Revelstoke Review'
  , 'RTR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Richmond Review'
  , 'RR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Saanich News'
  , 'SN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Salmon Arm Observer'
  , 'SAO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Smithers Interior News'
  , 'SIN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Sooke News Mirror'
  , 'SNM'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'South Delta Leader'
  , 'SDL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Summerland Review'
  , 'SR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Surrey North Delta Leader'
  , 'SL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Terrace Standard'
  , 'TSTD'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Chilliwack Progress'
  , 'TCP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Tri-City News'
  , 'TCN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vanderhoof Omineca Express'
  , 'VOE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vernon Morning Star'
  , 'VMS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Victoria News'
  , 'VN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Victoria Weekend'
  , 'VW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'WestEnder'
  , 'WESTENDER'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Williams Lake Tribune'
  , 'WLT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC Online'
  , 'CBCO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Daily News (Nanaimo)'
  , 'NANAIMO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Lake Country Calendar'
  , 'LCC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Caledonia Courier'
  , 'CC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKFR'
  , 'CKFR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Nelson Daily News'
  , 'NDN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Canadian Press Wire'
  , 'CPNEWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Gulf Islands Driftwood'
  , 'GID'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFIS'
  , 'CFIS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBCV'
  , 'CBCV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBTK'
  , 'CBTK'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBU'
  , 'CBU'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBYG'
  , 'CBYG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBUT'
  , 'CBUT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The New York Times'
  , 'NYT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Daily Telegraph'
  , 'DTL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKNW Online'
  , 'CKNW ONLINE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Montreal Gazette'
  , 'MG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Toronto Star'
  , 'TS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKWX Online'
  , 'CKWX ONLINE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Seattle PI Online'
  , 'SPIO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Georgia Straight Online'
  , 'GSO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver Province Online'
  , 'PROVO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Globe and Mail Online'
  , 'GMO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Victoria Buzz'
  , 'VBUZZ'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'KXLY'
  , 'KXLY'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Global News: BC 1'
  , 'BC 1'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Boundary Creek Times'
  , 'BCT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kimberley Bulletin'
  , 'KDB'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Peachland View'
  , 'PV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Conservative Party'
  , 'BC CONS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'News Kamloops'
  , 'NEWKAM'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Sierra Club of BC'
  , 'SIERRA'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Webcast'
  , 'WEBCAST'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Squamish Chief'
  , 'SC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver Sun Online'
  , 'SUN ONLINE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'HuffPostBC'
  , 'HUFFPOSTBC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJVB Online'
  , 'CJVB ONLINE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKSP'
  , 'CKSP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Orca'
  , 'ORCA'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKVU'
  , 'CKVU'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBX'
  , 'CBX'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kelowna Westside Weekly'
  , 'KWW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKSP Sameer Kaushal'
  , 'CKSP SAMEER'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Alaska Highway News'
  , 'AHN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Northerner'
  , 'FSJN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKFU'
  , 'CKFU'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver 24 hrs'
  , '24HRS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'StarMetro'
  , 'STARMETRO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Blacks Newsgroup'
  , 'BCNG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Meltwater'
  , 'MELTWATER'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Union of BC Indian Chiefs'
  , 'UBCIC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'TVS - Talentvision'
  , 'TVS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CityCaucus.com'
  , 'CITYC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Hospital Employees'' Union'
  , 'HEU'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Health Coalition'
  , 'BCHC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHMB'
  , 'CHMB'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJRJ'
  , 'CJRJ'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'KVRI'
  , 'KVRI'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'British Columbia Nurses'' Union'
  , 'BCNU'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'PTC'
  , 'PTC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver Island Free Daily'
  , 'VIFD'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Tyee'
  , 'TYEE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Canadian Union of Public Employees'
  , 'CUPE BC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'APTN'
  , 'APTN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Social Media'
  , 'BCPOLI'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKPG Online'
  , 'CKPG ONLINE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHNM'
  , 'CHNM'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'British Columbia Federation of Labour'
  , 'BC FED'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Grand Forks Gazette'
  , 'GFG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Teachers'' Federation'
  , 'BCTF'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Public Eye Online'
  , 'PEO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Local News'
  , 'BCLN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBCIndigNews'
  , 'CBCINDIGNEWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBCBCNews'
  , 'CBCBCNEWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Narwhal'
  , 'NAR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Infotel'
  , 'INFOTEL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
    'KNKX' -- name
  , 'KNKX' -- code
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

INSERT INTO public.source (
  "name"
  , "code"
  , "short_name"
  , "description"
  , "is_enabled"
  , "product_id"
  , "auto_transcribe"
  , "disable_transcribe"
  , "license_id"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Times Colonist (Victoria)'
  , 'TC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , 1 -- product_id
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver Sun'
  , 'SUN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , 1 -- product_id
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'National Post'
  , 'POST'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , 1 -- product_id
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Province'
  , 'PROVINCE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , 1 -- product_id
  , false
  , false
  , 3 -- license_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
