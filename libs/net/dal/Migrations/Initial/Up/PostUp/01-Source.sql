DO $$
DECLARE dailyPrintId INT := (SELECT "id" FROM public.product WHERE Name = 'Daily Print'); -- product_id

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
  , "product_id"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'Daily Hive'
  , 'DAILYHIVE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 7 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CJCN'
  , 'CJCN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'The Georgia Straight'
  , 'GEORGIA STRAIGHT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 7 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Burnaby Now'
  , 'BNOW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Chilliwack Times'
  , 'CTIMES'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Tri-Cities Now'
  , 'TCNOW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Richmond News'
  , 'RNEWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Castanet'
  , 'CASTANET'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 5 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Coast Mountain News'
  , 'CMN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Salmon Arm Lakeshore News'
  , 'SALN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Indo-Canadian Voice'
  , 'ICV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 5 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Macleans'
  , 'MACL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'iPolitics'
  , 'IPOLY'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 7 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Cranbrook Townsman'
  , 'CDT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Delta Optimist'
  , 'DO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Langley Advance Times'
  , 'LA'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Nelson Star'
  , 'NS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'North Shore News'
  , 'NSN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Rossland News'
  , 'RN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Surrey Now-Leader'
  , 'SURN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Royal City Record'
  , 'RCR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Announcement'
  , 'ANNOUNCE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Media Availability'
  , 'MEDAV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Scrum'
  , 'SCRUM'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Speech'
  , 'SPEECH'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Cloverdale Reporter'
  , 'CRR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Creston Valley Advance'
  , 'CVA'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'The Hook'
  , 'HOOK'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CHKG'
  , 'CHKG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CTV Online'
  , 'CTV ONLINE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Canadian Centre for Policy Alternatives'
  , 'CCPA'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Cowichan Valley Citizen'
  , 'CVC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Haida Gwaii Observer'
  , 'HGO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'North Delta Reporter'
  , 'NDR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Tofino-Ucluelet Westerly News'
  , 'TUWN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'BNN'
  , 'BNN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 7 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Trail Daily Times'
  , 'TDT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Comox Valley Echo'
  , 'CVE'
  , '' -- short_name
  , '' -- description
  , false -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Abbotsford Times'
  , 'AT'
  , '' -- short_name
  , '' -- description
  , false -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Edmonton Journal'
  , 'EJ'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Calgary Herald'
  , 'CH'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Oliver Chronicle'
  , 'APOC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'South Okanagan Times Chronicle'
  , 'TTC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Coast Reporter'
  , 'CORE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Dawson Creek Mirror'
  , 'DCMR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Columbia Valley Pioneer'
  , 'CVP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Chemainus Valley Courier'
  , 'CHVC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 4 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Opinion 250'
  , 'O250'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Fernie Free Press'
  , 'TFP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 4 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Keremeos Review'
  , 'KR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CFTV'
  , 'CFTV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CJVB'
  , 'CJVB'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Ming Pao News'
  , 'MING PAO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , -2 -- sort_order
  , ''
  , ''
), (
  'Sing Tao Daily'
  , 'SING TAO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Vancouver is Awesome'
  , 'VIAWE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'KRPI'
  , 'KRPI'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 4 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Calgary Herald (Print Edition)'
  , 'CHPE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Business in Vancouver'
  , 'BIV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CHBC'
  , 'CHBC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CFJC'
  , 'CFJC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CHNL'
  , 'CHNL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CFNR'
  , 'CFNR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CKYE'
  , 'CKYE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Bowen Island Undercurrent'
  , 'BIU'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Burns Lake Lakes District News'
  , 'BLLDN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 4 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Chilliwack Progress'
  , 'CP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 4 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Kelowna Capital News'
  , 'KCN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 4 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Monday Magazine'
  , 'MM'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'North Island Gazette'
  , 'NIG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 4 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Sicamous Eagle Valley News'
  , 'SEVN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 4 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Similkameen Spotlight'
  , 'SIMSP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'The Daily News (Prince Rupert)'
  , 'PRDN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CTV'
  , 'CTV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Castlegar News'
  , 'CN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Hope Standard'
  , 'HS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'North Island MidWeek'
  , 'NIMW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Prince Rupert Northern View'
  , 'NV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Alberni Valley News'
  , 'AVN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'The Valley Sentinel'
  , 'VS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Trail-Rossland News'
  , 'TRN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Vancouver Courier'
  , 'VC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Kootenay Western Star'
  , 'KWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CFAX'
  , 'CFAX'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'AV Archive'
  , 'ARCHIVE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Regional'
  , 'REGIONAL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CKNW'
  , 'CKNW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CHEK'
  , 'CHEK'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CHAN'
  , 'CHAN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CBC'
  , 'CBC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CIVT'
  , 'CIVT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Globe and Mail'
  , 'GLOBE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 5 -- license_id
  , null -- product_id
  , -5 -- sort_order
  , ''
  , ''
), (
  'Today''s Edition'
  , 'TE'
  , '' -- short_name
  , '' -- description
  , false -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CC News'
  , 'CCNEWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'The Daily News (Kamloops)'
  , 'KAMLOOPS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CKPG'
  , 'CKPG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'SHAW'
  , 'SHAW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CIVI'
  , 'CIVI'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Prince George Citizen'
  , 'PGC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Open Cabinet'
  , 'OPENCABINET'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'The Daily Courier (Kelowna)'
  , 'KELOWNA'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , -3 -- sort_order
  , ''
  , ''
), (
  'CKWX'
  , 'CKWX'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Press Theatre'
  , 'PRESS THEATRE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  '100 Mile House Free Press'
  , '100MILE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Abbotsford News'
  , 'ABBNEWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Agassiz-Harrison Observer'
  , 'AGASSIZ'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Aldergrove Star'
  , 'ALDERSTAR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Arrow Lakes News'
  , 'ARROWLAKE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Ashcroft Cache Creek Journal'
  , 'ASHJOUR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Barriere Star Journal'
  , 'BARRSTARR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Burnaby NewsLeader'
  , 'BNL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Campbell River Mirror'
  , 'CRM'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Clearwater Times'
  , 'CT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Comox Valley Record'
  , 'CCVR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Cowichan News Leader Pictorial'
  , 'CNLP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Esquimalt News'
  , 'EN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Fort Saint James Courier'
  , 'FSJC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Golden Star'
  , 'GS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Goldstream News Gazette'
  , 'GG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Houston Today'
  , 'HT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Invermere Valley Echo'
  , 'IVE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Kamloops This Week'
  , 'KTW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Kitimat Northern Sentinel'
  , 'KS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Kootenay News Advertiser'
  , 'KNA'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Ladysmith Chronicle'
  , 'LC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Lake Cowichan Gazette'
  , 'LCG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Lakes District News'
  , 'LDN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Langley Times'
  , 'LT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Maple Ridge-Pitt Meadows News'
  , 'MRN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Merritt Herald'
  , 'MH'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Mission City Record'
  , 'MCR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Nanaimo News Bulletin'
  , 'NNB'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'New Westminster News Leader'
  , 'NWNL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'North Island Weekender'
  , 'NIW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'North Shore Outlook'
  , 'NSO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Oak Bay News'
  , 'OBN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Parksville Qualicum Beach News'
  , 'PQN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Peace Arch News'
  , 'PAN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Peninsula News Review'
  , 'PNR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Penticton Western News'
  , 'PW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Port Hardy North Island Gazette'
  , 'PHNIG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Prince George Free Press'
  , 'PGFP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Princeton Similkameen Spotlight'
  , 'PSS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'BC Government and Service Employees'' Union'
  , 'BCGEU'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'New Westminster Record'
  , 'NWR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CBYK'
  , 'CBYK'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CorpCal'
  , 'CorpCal'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'iNFOnews'
  , 'INFONEWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 7 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Link'
  , 'LINK'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 5 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Quesnel Cariboo Observer'
  , 'QCO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Revelstoke Review'
  , 'RTR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Richmond Review'
  , 'RR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Saanich News'
  , 'SN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Salmon Arm Observer'
  , 'SAO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Smithers Interior News'
  , 'SIN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Sooke News Mirror'
  , 'SNM'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'South Delta Leader'
  , 'SDL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Summerland Review'
  , 'SR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Surrey North Delta Leader'
  , 'SL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Terrace Standard'
  , 'TSTD'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'The Chilliwack Progress'
  , 'TCP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Tri-City News'
  , 'TCN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Vanderhoof Omineca Express'
  , 'VOE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Vernon Morning Star'
  , 'VMS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Victoria News'
  , 'VN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Victoria Weekend'
  , 'VW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'WestEnder'
  , 'WESTENDER'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Williams Lake Tribune'
  , 'WLT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CBC Online'
  , 'CBCO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 5 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'The Daily News (Nanaimo)'
  , 'NANAIMO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Lake Country Calendar'
  , 'LCC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Caledonia Courier'
  , 'CC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CKFR'
  , 'CKFR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Nelson Daily News'
  , 'NDN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Canadian Press Wire'
  , 'CPNEWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Gulf Islands Driftwood'
  , 'GID'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CFIS'
  , 'CFIS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CBCV'
  , 'CBCV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CBTK'
  , 'CBTK'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CBU'
  , 'CBU'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CBYG'
  , 'CBYG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CBUT'
  , 'CBUT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'The New York Times'
  , 'NYT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'The Daily Telegraph'
  , 'DTL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CKNW Online'
  , 'CKNW ONLINE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 7 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Montreal Gazette'
  , 'MG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'The Toronto Star'
  , 'TS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CKWX Online'
  , 'CKWX ONLINE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 7 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Seattle PI Online'
  , 'SPIO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Georgia Straight Online'
  , 'GSO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 7 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Vancouver Province Online'
  , 'PROVO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Globe and Mail Online'
  , 'GMO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 5 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Victoria Buzz'
  , 'VBUZZ'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 7 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'KXLY'
  , 'KXLY'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 7 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Global News: BC 1'
  , 'BC 1'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Boundary Creek Times'
  , 'BCT'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Kimberley Bulletin'
  , 'KDB'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Peachland View'
  , 'PV'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'BC Conservative Party'
  , 'BC CONS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'News Kamloops'
  , 'NEWKAM'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Sierra Club of BC'
  , 'SIERRA'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Webcast'
  , 'WEBCAST'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'The Squamish Chief'
  , 'SC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Vancouver Sun Online'
  , 'SUN ONLINE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'HuffPostBC'
  , 'HUFFPOSTBC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CJVB Online'
  , 'CJVB ONLINE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 7 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CKSP'
  , 'CKSP'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Orca'
  , 'ORCA'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CKVU'
  , 'CKVU'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CBX'
  , 'CBX'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Kelowna Westside Weekly'
  , 'KWW'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 4 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CKSP Sameer Kaushal'
  , 'CKSP SAMEER'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Alaska Highway News'
  , 'AHN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'The Northerner'
  , 'FSJN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CKFU'
  , 'CKFU'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Vancouver 24 hrs'
  , '24HRS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'StarMetro'
  , 'STARMETRO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Blacks Newsgroup'
  , 'BCNG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Meltwater'
  , 'MELTWATER'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 6 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Union of BC Indian Chiefs'
  , 'UBCIC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'TVS - Talentvision'
  , 'TVS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CityCaucus.com'
  , 'CITYC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Hospital Employees'' Union'
  , 'HEU'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'BC Health Coalition'
  , 'BCHC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CHMB'
  , 'CHMB'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CJRJ'
  , 'CJRJ'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'KVRI'
  , 'KVRI'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 7 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'British Columbia Nurses'' Union'
  , 'BCNU'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'PTC'
  , 'PTC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Vancouver Island Free Daily'
  , 'VIFD'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'The Tyee'
  , 'TYEE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 5 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Canadian Union of Public Employees'
  , 'CUPE BC'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'APTN'
  , 'APTN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Social Media'
  , 'BCPOLI'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CKPG Online'
  , 'CKPG ONLINE'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 7 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CHNM'
  , 'CHNM'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 2 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'British Columbia Federation of Labour'
  , 'BC FED'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Grand Forks Gazette'
  , 'GFG'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'BC Teachers'' Federation'
  , 'BCTF'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Public Eye Online'
  , 'PEO'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'BC Local News'
  , 'BCLN'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CBCIndigNews'
  , 'CBCINDIGNEWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'CBCBCNews'
  , 'CBCBCNEWS'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Narwhal'
  , 'NAR'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 7 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Infotel'
  , 'INFOTEL'
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
    'KNKX' -- name
  , 'KNKX' -- code
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Times Colonist (Victoria)' -- name
  , 'TC' -- code
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , dailyPrintId -- product_id
  , -6 -- sort_order
  , ''
  , ''
), (
  'Vancouver Sun' -- name
  , 'SUN' -- code
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , dailyPrintId -- product_id
  , -8 -- sort_order
  , ''
  , ''
), (
  'National Post' -- name
  , 'POST' -- code
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , dailyPrintId -- product_id
  , -4 -- sort_order
  , ''
  , ''
), (
  'The Province' -- name
  , 'PROVINCE' -- code
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , dailyPrintId -- product_id
  , -7 -- sort_order
  , ''
  , ''
), (
  'Ha-Shilth-Sa' -- name
  , 'HaShilthSa' -- code
  , '' -- short_name
  , '' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 3 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Capital Daily' -- name
  , 'CAPDAILY' -- code
  , '' -- short_name
  , 'HTML https://www.capitaldaily.ca/' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'The Westshore' -- name
  , 'WESTSHORE' -- code
  , '' -- short_name
  , 'HTML https://www.thewestshore.ca/' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Victoria Tech Journal' -- name
  , 'VTJ' -- code
  , '' -- short_name
  , 'HTML https://www.victechjournal.com/' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Burnaby Beacon' -- name
  , 'BBEACON' -- code
  , '' -- short_name
  , 'HTML https://burnabybeacon.com/' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'Fraser Valley Current' -- name
  , 'FVC' -- code
  , '' -- short_name
  , 'HTML https://fvcurrent.com/' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
), (
  'New West Anchor' -- name
  , 'NWA' -- code
  , '' -- short_name
  , 'HTML https://www.newwestanchor.com/' -- description
  , true -- is_enabled
  , false -- auto_transcribe
  , false -- disable_transcribe
  , 1 -- license_id
  , null -- product_id
  , 0 -- sort_order
  , ''
  , ''
);

END $$;
