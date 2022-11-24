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
  'Syndication' -- 1
  , 'RSS/ATOM feeds from newswire services such as Castanet and CP News'
  , true -- is_enabled
  , 3 -- content_type - Story
  , false -- auto_transcribe
  , false -- disable_transcribe
  , ''
  , ''
), (
  'Audio' -- 2
  , 'Audio streams, or files such as talk shows/commentary focused on #BCPolicy'
  , true -- is_enabled
  , 0 -- content_type - Snippet
  , false -- auto_transcribe
  , false -- disable_transcribe
  , ''
  , ''
), (
  'Video' -- 3
  , 'Video streams, or files including 15 television stations'
  , true -- is_enabled
  , 0 -- content_type - Snippet
  , false -- auto_transcribe
  , false -- disable_transcribe
  , ''
  , ''
), (
  'Image' -- 4
  , 'Images imported from external sources.'
  , true -- is_enabled
  , 2 -- content_type - Image
  , false -- auto_transcribe
  , false -- disable_transcribe
  , ''
  , ''
), (
  'Front Page' -- 5
  , 'Front page images from newspapers.'
  , true -- is_enabled
  , 2 -- content_type - Image
  , false -- auto_transcribe
  , false -- disable_transcribe
  , ''
  , ''
), (
  'Corporate Calendar' -- 6
  , 'Screenshot from the Corporate Calendar Look Ahead Report'
  , true -- is_enabled
  , 2 -- content_type - Image
  , false -- auto_transcribe
  , false -- disable_transcribe
  , ''
  , ''
), (
  'Paper' -- 7
  , 'Text files from newspapers, 2 national dailies, 11 provincial dailies'
  , true -- is_enabled
  , 1 -- content_type - Print Content
  , false -- auto_transcribe
  , false -- disable_transcribe
  , ''
  , ''
), (
  'HTML' -- 8
  , 'Webpage scraping for news'
  , true -- is_enabled
  , 3 -- content_type - Story
  , false -- auto_transcribe
  , false -- disable_transcribe
  , ''
  , ''
), (
  'Social Media' -- 9
  , 'Social ingest platforms such as Twitter, Facebook'
  , true -- is_enabled
  , 3 -- content_type - Story
  , false -- auto_transcribe
  , false -- disable_transcribe
  , ''
  , ''
);

END $$;
