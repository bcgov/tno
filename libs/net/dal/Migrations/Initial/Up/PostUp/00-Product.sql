DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.product (
  "name"
  , "description"
  , "is_enabled"
  , "sort_order"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Daily Print' -- 1
  , 'Loads text and images - Alerts only headlines (was Newspaper)'
  , true
  , 100
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Weekly Print' -- 2
  , 'Loads text and images - Alerts only headlines (was Regional)'
  , true
  , 100
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Online Print' -- 3
  , 'Loads text and images - Alerts all listed fields'
  , true
  , 100
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'News Radio' -- 4
  , 'Loads summary and audio player - Alerts summary unless transcript is present'
  , true
  , 100
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Talk Radio' -- 5
  , 'Loads summary and audio player - Alerts summary unless transcript is present'
  , true
  , 100
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'TV / Video News' -- 6
  , 'Loads summary and video player - Alerts summary unless transcript is present (was TV)'
  , true
  , 100
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CP Wire' -- 7
  , 'Loads text and images - Alerts all listed fields (was CP News)'
  , true
  , 10
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'AV Archive' -- 8
  , 'Once binary file expires - includes RSN for easy retrieval from DVD archive'
  , true
  , 100
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Speeches & Scrums' -- 9
  , 'Summary or transcript and audio or video player depending upon file attached - Summary is deleted once Transcript is added - Alerts summary unless transcript is present - restricted user access'
  , true
  , 100
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Live stream' -- 10
  , 'Embedded press theatre URL - Alerts summary and URL link - can be password protected (was Webcast)'
  , true
  , 100
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Front Page' -- 11
  , 'JPGs of front pages of 5 Daily Papers'
  , true
  , 100
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Corporate Calendar' -- 12
  , 'Screenshot of corporate calendar - page 2'
  , true
  , 100
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BiV' -- 13
  , 'Business in Vancouver'
  , true
  , 40
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Castanet' -- 14
  , ''
  , true
  , 70
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Daily Hive' -- 15
  , ''
  , true
  , 30
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ha-Shilth-Sa' -- 16
  , 'Canada''s Oldest First Nations Newspaper https://hashilthsa.com/news'
  , true
  , 80
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'iPolitics' -- 17
  , ''
  , true
  , 20
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Georgia Straight' -- 18
  , ''
  , true
  , 50
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'iNFOnews' -- 19
  , ''
  , true
  , 60
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Advertising Campaign' -- 20
  , ''
  , true
  , 600
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;












