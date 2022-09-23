DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.category (
  "name"
  , "is_enabled"
  , "category_type"
  , "auto_transcribe"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Malcolmson - suicide prevention (ProActive Comms)'
  , true -- is_enabled
  , 1 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Repeat offender recommendations (ProActive Comms)'
  , true -- is_enabled
  , 1 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Alberta Is Calling'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ambulance waits'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'ArriveCan app'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC NDP leadership'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC crime'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC housing costs'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'COVID-19 update'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Climate change'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Dam release lawsuit'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Daycare death sentencing'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'ER closure'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Gang violence'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Health care decline'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'IIO investigation'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Inflation concerns'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Mackenzie - seniors report'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Municipal election'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Old-growth protesters'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Overdose crisis'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Port strike'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Street lawlessness'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Surrey South by-election'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Truth and Reconciliation Day'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ukraine invasion'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Wildfire season'
  , true -- is_enabled
  , 0 -- category_type
  , false -- auto_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
