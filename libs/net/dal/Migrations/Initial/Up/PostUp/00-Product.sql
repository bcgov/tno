DO $$
BEGIN

INSERT INTO public.product (
  "name"
  , "description"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'Daily Print' -- 1
  , 'Loads text and images - Alerts only headlines (was Newspaper)'
  , true
  , 100
  , ''
  , ''
), (
  'Weekly Print' -- 2
  , 'Loads text and images - Alerts only headlines (was Regional)'
  , true
  , 100
  , ''
  , ''
), (
  'Online Print' -- 3
  , 'Loads text and images - Alerts all listed fields'
  , true
  , 100
  , ''
  , ''
), (
  'News Radio' -- 4
  , 'Loads summary and audio player - Alerts summary unless transcript is present'
  , true
  , 100
  , ''
  , ''
), (
  'Talk Radio' -- 5
  , 'Loads summary and audio player - Alerts summary unless transcript is present'
  , true
  , 100
  , ''
  , ''
), (
  'TV / Video News' -- 6
  , 'Loads summary and video player - Alerts summary unless transcript is present (was TV)'
  , true
  , 100
  , ''
  , ''
), (
  'CP Wire' -- 7
  , 'Loads text and images - Alerts all listed fields (was CP News)'
  , true
  , 10
  , ''
  , ''
), (
  'AV Archive' -- 8
  , 'Once binary file expires - includes RSN for easy retrieval from DVD archive'
  , true
  , 100
  , ''
  , ''
), (
  'Speeches & Scrums' -- 9
  , 'Summary or transcript and audio or video player depending upon file attached - Summary is deleted once Transcript is added - Alerts summary unless transcript is present - restricted user access'
  , true
  , 100
  , ''
  , ''
), (
  'Live stream' -- 10
  , 'Embedded press theatre URL - Alerts summary and URL link - can be password protected (was Webcast)'
  , true
  , 100
  , ''
  , ''
), (
  'Front Page' -- 11
  , 'JPGs of front pages of 5 Daily Papers'
  , true
  , 100
  , ''
  , ''
), (
  'Corporate Calendar' -- 12
  , 'Screenshot of corporate calendar - page 2'
  , true
  , 100
  , ''
  , ''
), (
  'BiV' -- 13
  , 'Business in Vancouver'
  , true
  , 40
  , ''
  , ''
), (
  'Castanet' -- 14
  , ''
  , true
  , 70
  , ''
  , ''
), (
  'Daily Hive' -- 15
  , ''
  , true
  , 30
  , ''
  , ''
), (
  'Ha-Shilth-Sa' -- 16
  , 'Canada''s Oldest First Nations Newspaper https://hashilthsa.com/news'
  , true
  , 80
  , ''
  , ''
), (
  'iPolitics' -- 17
  , ''
  , true
  , 20
  , ''
  , ''
), (
  'Georgia Straight' -- 18
  , ''
  , true
  , 50
  , ''
  , ''
), (
  'iNFOnews' -- 19
  , ''
  , true
  , 60
  , ''
  , ''
), (
  'Advertising Campaign' -- 20
  , ''
  , true
  , 99
  , ''
  , ''
), (
  'Narwhal' -- 21
  , ''
  , true
  , 100
  , ''
  , ''
), (
  'The Georgia Straight' -- 22
  , ''
  , true
  , 100
  , ''
  , ''
);

END $$;












