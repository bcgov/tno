DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
DECLARE CBC_CAPTURE_ID integer := (SELECT id FROM public.data_source WHERE public.data_source.code = 'CBCV-CAPTURE');
DECLARE CBC_CLIP_ID integer := (SELECT id FROM public.data_source WHERE public.data_source.code = 'CBCV');
DECLARE CASTANET_ID integer := (SELECT id FROM public.data_source WHERE public.data_source.code = 'CASTANET');
BEGIN

IF (CBC_CAPTURE_ID IS NOT NULL) THEN
INSERT INTO public.data_source_schedule (
  "data_source_id"
  , "schedule_id"
  , "created_by_id"
  , "created_by"
  , "created_on"
  , "updated_by_id"
  , "updated_by"
  , "updated_on"
  , "version"
) VALUES (
  CBC_CAPTURE_ID  -- data_source_id
  , 1  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , 0
);
END IF;

IF (CBC_CLIP_ID IS NOT NULL) THEN
INSERT INTO public.data_source_schedule (
  "data_source_id"
  , "schedule_id"
  , "created_by_id"
  , "created_by"
  , "created_on"
  , "updated_by_id"
  , "updated_by"
  , "updated_on"
  , "version"
) VALUES (
  CBC_CLIP_ID  -- data_source_id
  , 2  -- schedule_id
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , DEFAULT_USER_ID
  , ''
  , CURRENT_TIMESTAMP
  , 0
);
END IF;

IF (CASTANET_ID IS NOT NULL) THEN
  INSERT INTO public.data_source_schedule (
    "data_source_id"
    , "schedule_id"
    , "created_by_id"
    , "created_by"
    , "created_on"
    , "updated_by_id"
    , "updated_by"
    , "updated_on"
    , "version"
  ) VALUES (
    CASTANET_ID  -- data_source_id
    , 3  -- schedule_id
    , DEFAULT_USER_ID
    , ''
    , CURRENT_TIMESTAMP
    , DEFAULT_USER_ID
    , ''
    , CURRENT_TIMESTAMP
    , 0
  );
END IF;

END $$;