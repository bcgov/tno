DO $$
BEGIN

INSERT INTO public.ingest_type (
  "name"
  , "description"
  , "is_enabled"
  , "content_type"
  , "auto_transcribe"
  , "disable_transcribe"
  , "created_by"
  , "updated_by"
) VALUES (
  'TNO-Snippet' -- 10
  , 'Snippet migration from TNO 1.0'
  , true -- is_enabled
  , 0 -- content_type - Snippet
  , false -- auto_transcribe
  , false -- disable_transcribe
  , ''
  , ''
),(
  'TNO-PrintContent' -- 11
  , 'Print Content migration from TNO 1.0'
  , true -- is_enabled
  , 1 -- content_type - PrintContent
  , false -- auto_transcribe
  , false -- disable_transcribe
  , ''
  , ''
),(
  'TNO-Image' -- 12
  , 'Image migration from TNO 1.0'
  , true -- is_enabled
  , 2 -- content_type - Image
  , false -- auto_transcribe
  , false -- disable_transcribe
  , ''
  , ''
),(
  'TNO-Story' -- 13
  , 'Story migration from TNO 1.0'
  , true -- is_enabled
  , 3 -- content_type - Story
  , false -- auto_transcribe
  , false -- disable_transcribe
  , ''
  , ''
);

END $$;
