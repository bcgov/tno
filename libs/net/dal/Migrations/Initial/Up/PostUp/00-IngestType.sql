DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.ingest_type (
  "name"
  , "description"
  , "is_enabled"
  , "content_type"
  , "auto_transcribe"
  , "disable_transcribe"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Syndication' -- 1
  , 'RSS/ATOM feeds from newswire services such as Castanet and CP News'
  , true -- is_enabled
  , 3 -- content_type - Story
  , false -- auto_transcribe
  , false -- disable_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Audio' -- 2
  , 'Audio streams, or files such as talk shows/commentary focused on #BCPolicy'
  , true -- is_enabled
  , 0 -- content_type - Snippet
  , false -- auto_transcribe
  , false -- disable_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Video' -- 3
  , 'Video streams, or files including 15 television stations'
  , true -- is_enabled
  , 0 -- content_type - Snippet
  , false -- auto_transcribe
  , false -- disable_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Image' -- 4
  , 'Images imported from external sources.'
  , true -- is_enabled
  , 2 -- content_type - Image
  , false -- auto_transcribe
  , false -- disable_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Front Page' -- 5
  , 'Front page images from newspapers.'
  , true -- is_enabled
  , 2 -- content_type - Image
  , false -- auto_transcribe
  , false -- disable_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Corporate Calendar' -- 6
  , 'Screenshot from the Corporate Calendar Look Ahead Report'
  , true -- is_enabled
  , 2 -- content_type - Image
  , false -- auto_transcribe
  , false -- disable_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Paper' -- 7
  , 'Text files from newspapers, 2 national dailies, 11 provincial dailies'
  , true -- is_enabled
  , 1 -- content_type - Print Content
  , false -- auto_transcribe
  , false -- disable_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'HTML' -- 8
  , 'Webpage scraping for news'
  , true -- is_enabled
  , 3 -- content_type - Story
  , false -- auto_transcribe
  , false -- disable_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Social Media' -- 9
  , 'Social ingest platforms such as Twitter, Facebook'
  , true -- is_enabled
  , 3 -- content_type - Story
  , false -- auto_transcribe
  , false -- disable_transcribe
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
