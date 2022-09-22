DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.product (
  "name"
  , "description"
  , "is_enabled"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Daily Print' -- 1
  , 'Loads text and images - Alerts only headlines (was Newspaper)'
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Weekly Print' -- 2
  , 'Loads text and images - Alerts only headlines (was Regional)'
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Online Print' -- 3
  , 'Loads text and images - Alerts all listed fields'
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'News Radio' -- 4
  , 'Loads summary and audio player - Alerts summary unless transcript is present'
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Talk Radio' -- 5
  , 'Loads summary and audio player - Alerts summary unless transcript is present'
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Video News' -- 6
  , 'Loads summary and video player - Alerts summary unless transcript is present (was TV)'
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Wire' -- 7
  , 'Loads text and images - Alerts all listed fields (was CP News)'
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'AV Archive' -- 8
  , 'Once binary file expires - includes RSN for easy retrieval from DVD archive'
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Speeches & Scrums' -- 9
  , 'Summary or transcript and audio or video player depending upon file attached - Summary is deleted once Transcript is added - Alerts summary unless transcript is present - restricted user access'
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Live stream' -- 10
  , 'Embedded press theatre URL - Alerts summary and URL link - can be password protected (was Webcast)'
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Front Page' -- 11
  , 'Newspaper front page images'
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Corporate Calendar' -- 12
  , 'Screenshot of corporate calendar'
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Today''s Edition' -- 13
  , ''
  , true
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;












