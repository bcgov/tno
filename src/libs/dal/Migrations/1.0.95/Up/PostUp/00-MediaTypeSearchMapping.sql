DO $$
BEGIN

-- News Radio & Talk Radio
INSERT INTO public.source_media_type_search_mapping (
  "source_id"
  , "media_type_id"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
)
SELECT
  source_id
  , media_type_id
  , '' as created_by
  , CURRENT_TIMESTAMP as created_on
  , '' as updated_by
  , CURRENT_TIMESTAMP as updated_on

FROM (
	SELECT "id" as media_type_id
	FROM public.media_type
  WHERE Name in ('News Radio', 'Talk Radio')
) a
	FULL OUTER JOIN (
	SELECT "id" as source_id
	FROM public.source
  WHERE "code" in
    ('CBCV', 'CBCTK', 'CBU', 'CBYG', 'CBYK', 'CFAX', 'CHMB', 'CHNL' ,'CJCN', 'CJVB',
     'CKFR', 'CKFU', 'CKNW', 'CKSP', 'CKWX', 'CKYE', 'KRPI', 'KVRI', 'KXLY')
) b
   ON 1 = 1;
-- conflict should never happen because the table doesn't exist yet
-- ON CONFLICT ON CONSTRAINT PK_source_media_type_search_mapping DO NOTHING;

-- CP News
INSERT INTO public.source_media_type_search_mapping (
  "source_id"
  , "media_type_id"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
)
SELECT
  source_id
  , media_type_id
  , '' as created_by
  , CURRENT_TIMESTAMP as created_on
  , '' as updated_by
  , CURRENT_TIMESTAMP as updated_on

FROM (
	SELECT "id" as media_type_id
	FROM public.media_type
  WHERE Name in ('CP Wire')
) a
	FULL OUTER JOIN (
	SELECT "id" as source_id
	FROM public.source
  WHERE "code" in
    ('CPNEWS')
) b
   ON 1 = 1;

-- Daily Print
INSERT INTO public.source_media_type_search_mapping (
  "source_id"
  , "media_type_id"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
)
SELECT
  source_id
  , media_type_id
  , '' as created_by
  , CURRENT_TIMESTAMP as created_on
  , '' as updated_by
  , CURRENT_TIMESTAMP as updated_on

FROM (
	SELECT "id" as media_type_id
	FROM public.media_type
  WHERE Name in ('Daily Print')
) a
	FULL OUTER JOIN (
	SELECT "id" as source_id
	FROM public.source
  WHERE "code" in
    ('CH', 'CHPE', 'EJ', 'GLOBE', 'KAMLOOPS', 'MG', 'NANAIMO', 'NDN', 'PRDN',
    'PROVINCE', 'SING TAO', 'SUN', 'TC', 'TDT')
) b
   ON 1 = 1;

-- Events
INSERT INTO public.source_media_type_search_mapping (
  "source_id"
  , "media_type_id"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
)
SELECT
  source_id
  , media_type_id
  , '' as created_by
  , CURRENT_TIMESTAMP as created_on
  , '' as updated_by
  , CURRENT_TIMESTAMP as updated_on

FROM (
	SELECT "id" as media_type_id
	FROM public.media_type
  WHERE Name in ('Events')
) a
	FULL OUTER JOIN (
	SELECT "id" as source_id
	FROM public.source
  WHERE "code" in
    ('ANNOUNCE', 'MEDAV', 'PRESS THEATRE', 'SCRUM', 'SPEECH')
) b
   ON 1 = 1;

-- Online
INSERT INTO public.source_media_type_search_mapping (
  "source_id"
  , "media_type_id"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
)
SELECT
  source_id
  , media_type_id
  , '' as created_by
  , CURRENT_TIMESTAMP as created_on
  , '' as updated_by
  , CURRENT_TIMESTAMP as updated_on

FROM (
	SELECT "id" as media_type_id
	FROM public.media_type
  WHERE Name in ('Online')
) a
	FULL OUTER JOIN (
	SELECT "id" as source_id
	FROM public.source
  WHERE "code" in
    ('24HRS', 'BBEACON', 'BIV', 'CAPDAILY', 'CASTANET', 'CBCO', 'CITYC',
    'CJVB ONLINE', 'CKNW ONLINE', 'CKPG ONLINE', 'CKWX ONLINE', 'CR',
    'DAILYHIVE', 'FVC', 'GEORGIA STRAIGHT', 'GMO', 'GSO', 'HaShilthSa',
    'HOOK', 'HUFFPOSTBC', 'ICV', 'INFOTEL', 'LINK', 'NAR', 'NWA', 'O250',
    'ORCA', 'PEO', 'SPIO', 'TYEE', 'VBUZZ', 'VTJ', 'WESTSHORE')
) b
   ON 1 = 1;

-- TV / Video News
INSERT INTO public.source_media_type_search_mapping (
  "source_id"
  , "media_type_id"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
)
SELECT
  source_id
  , media_type_id
  , '' as created_by
  , CURRENT_TIMESTAMP as created_on
  , '' as updated_by
  , CURRENT_TIMESTAMP as updated_on

FROM (
	SELECT "id" as media_type_id
	FROM public.media_type
  WHERE Name in ('TV / Video News')
) a
	FULL OUTER JOIN (
	SELECT "id" as source_id
	FROM public.source
  WHERE "code" in
    ('APTN', 'BC 1', 'BNN', 'CBC', 'CFTV', 'CHAN', 'CHBC', 'CHEK', 'CHNM',
    'CIVI', 'CIVT', 'CTV', 'SHAW', 'TVS')
) b
   ON 1 = 1;

-- Weekly Print
INSERT INTO public.source_media_type_search_mapping (
  "source_id"
  , "media_type_id"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
)
SELECT
  source_id
  , media_type_id
  , '' as created_by
  , CURRENT_TIMESTAMP as created_on
  , '' as updated_by
  , CURRENT_TIMESTAMP as updated_on

FROM (
	SELECT "id" as media_type_id
	FROM public.media_type
  WHERE Name in ('Weekly Print')
) a
	FULL OUTER JOIN (
	SELECT "id" as source_id
	FROM public.source
  WHERE "code" in
    ('100MILE', 'ABBNEWS', 'AGASSIZ', 'AHN', 'ALDERSTAR', 'APOC',
    'ARROWLAKE', 'ASHJOUR', 'AT', 'AVN', 'BARRSTARR', 'BCLN', 'BCNG',
    'BCT', 'BIU', 'BLLDN', 'BNL', 'BNOW', 'CC', 'CCVR', 'CDT', 'CHVC',
    'CMN', 'CN', 'CNLP', 'CORE', 'CP', 'CRM', 'CRR', 'CT', 'CTIMES',
    'CVA', 'CVC', 'CVE', 'CVP', 'DCMR', 'DO', 'EN', 'FSJC', 'FSJN',
    'GFG', 'GG', 'GID', 'GS', 'HGO', 'HS', 'HT', 'IVE', 'KCN', 'KDB',
    'KNA', 'KR', 'KS', 'KTW', 'KWS', 'KWW', 'LA', 'LC', 'LCC', 'LCG',
    'LDN', 'LT', 'MACL', 'MCR', 'MELTWATER', 'MH', 'MM', 'MRN', 'NDR',
    'NEWKAM', 'NIG', 'NIMW', 'NIW', 'NNB', 'NS', 'NSN', 'NSO', 'NV',
    'NWNL', 'NWR', 'OBN', 'PAN', 'PGC', 'PGFP', 'PHNIG', 'PNR', 'PQN',
    'PSS', 'PV', 'PW', 'QCO', 'RCR', 'REGIONAL', 'RN', 'RNEWS', 'RR',
    'RTR', 'SALN', 'SAO', 'SC', 'SDL', 'SEVN', 'SIMSP', 'SIN', 'SL',
    'SN', 'SNM', 'SR', 'SURN', 'TCN', 'TCNOW', 'TCP', 'TFP', 'TSTD',
    'TTC', 'TUWN', 'VC', 'VS', 'WESTENDER', 'WLT')
) b
   ON 1 = 1;

END $$;
