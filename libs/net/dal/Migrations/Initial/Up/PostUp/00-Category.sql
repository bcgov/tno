DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.category (
  "name"
  , "description"
  , "is_enabled"
  , "category_type"
  , "auto_transcribe"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Malcolmson - suicide prevention'
  , '(ProActive Comms)' -- description
  , true -- is_enabled
  , 1 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Repeat offender recommendations'
  , '(ProActive Comms)' -- description
  , true -- is_enabled
  , 1 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Alberta Is Calling'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ambulance waits'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'ArriveCan app'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC NDP leadership'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC crime'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC housing costs'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'COVID-19'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Climate change'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Dam release lawsuit'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Daycare death sentencing'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'ER closure'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Gang violence'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Health care decline'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'IIO investigation'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Inflation concerns'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Mackenzie - seniors report'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Municipal elections'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Old-growth protesters'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Overdose crisis'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Port strike'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Street lawlessness'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Surrey South by-election'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Truth and Reconciliation Day'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ukraine invasion'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Wildfire season'
  , '' -- description
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Premier Eby / Government Transition'
  , '' -- description
  , true -- is_enabled
  , 1 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
