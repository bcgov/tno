DO $$
BEGIN

DELETE FROM public."setting" WHERE "name" = 'MorningReport';
DELETE FROM public."setting" WHERE "name" = 'SearchPageResultsHideSource';
DELETE FROM public."setting" WHERE "name" = 'SearchPageResultsShowPage';

INSERT INTO public."setting" (
  "name"
  , "description"
  , "value"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'AlertActionId' -- name
  , 'Primary key to the Alert Action.' -- description
  , (SELECT "id"::varchar(255) FROM public."action" WHERE "name" = 'Alert')
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT("name") DO UPDATE
SET "value" = (SELECT "id"::varchar(255) FROM public."action" WHERE "name" = 'Alert');

INSERT INTO public."setting" (
  "name"
  , "description"
  , "value"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'CommentaryActionId' -- name
  , 'Primary key to the Commentary Action.' -- description
  , (SELECT "id"::varchar(255) FROM public."action" WHERE "name" = 'Commentary')
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT("name") DO UPDATE
SET "value" = (SELECT "id"::varchar(255) FROM public."action" WHERE "name" = 'Commentary');

INSERT INTO public."setting" (
  "name"
  , "description"
  , "value"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'FeaturedActionId' -- name
  , 'Action id for marking a piece of content as "featured".' -- description
  , (SELECT "id"::varchar(255) FROM public."action" WHERE "name" = 'Featured Story')
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT("name") DO UPDATE
SET "value" = (SELECT "id"::varchar(255) FROM public."action" WHERE "name" = 'Featured Story');

INSERT INTO public."setting" (
  "name"
  , "description"
  , "value"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'TopStoryActionId' -- name
  , 'Primary key to the Top Story Action.' -- description
  , (SELECT "id"::varchar(255) FROM public."action" WHERE "name" = 'Top Story')
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT("name") DO UPDATE
SET "value" = (SELECT "id"::varchar(255) FROM public."action" WHERE "name" = 'Top Story');

INSERT INTO public."setting" (
  "name"
  , "description"
  , "value"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'EditorUrl' -- name
  , 'URL to the Editor application.' -- description
  , 'https://editor.mmi.gov.bc.ca'
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT DO NOTHING;

INSERT INTO public."setting" (
  "name"
  , "description"
  , "value"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'EventOfTheDayFolderId' -- name
  , 'Id of folder used by Event of the Day feature.' -- description
  , (SELECT "id"::varchar(255) FROM public."folder" WHERE "name" = 'Event of the Day' AND "owner_id" = 1)
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT("name") DO UPDATE
SET "value" = (SELECT "id"::varchar(255) FROM public."folder" WHERE "name" = 'Event of the Day' AND "owner_id" = 1);

INSERT INTO public."setting" (
  "name"
  , "description"
  , "value"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'FrontPageImageMediaTypeId' -- name
  , 'The front page image media type ID.' -- description
  , (SELECT "id"::varchar(255) FROM public."media_type" WHERE "name" = 'Front Page Images')
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT("name") DO UPDATE
SET "value" = (SELECT "id"::varchar(255) FROM public."media_type" WHERE "name" = 'Front Page Images');

INSERT INTO public."setting" (
  "name"
  , "description"
  , "value"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'SearchPageResultsNewWindow' -- name
  , 'These media types will show a link to open in a new window when displayed on search results on subscriber app. (Events, News Radio, Talk Radio, TV / Video News).' -- description
  , '4,5,6,9'
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT("name") DO UPDATE
SET "value" = '4,5,6,9';

INSERT INTO public."setting" (
  "name"
  , "description"
  , "value"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'FrontpageFilterId' -- name
  , 'Filter used for the subscriber "Today''s Front Pages" section.' -- description
  , (SELECT "id"::varchar(255) FROM public."filter" WHERE "name" = 'DAILY - Front page images' AND "owner_id" = 1)
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT("name") DO UPDATE
SET "value" = (SELECT "id"::varchar(255) FROM public."filter" WHERE "name" = 'DAILY - Front page images' AND "owner_id" = 1);

INSERT INTO public."setting" (
  "name"
  , "description"
  , "value"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'DefaultReportTemplateId' -- name
  , 'The default report template for subscriber reports.' -- description
  , (SELECT "id"::varchar(255) FROM public."report_template" WHERE "name" = 'Custom Report')
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT("name") DO UPDATE
SET "value" = (SELECT "id"::varchar(255) FROM public."report_template" WHERE "name" = 'Custom Report');

END $$;
