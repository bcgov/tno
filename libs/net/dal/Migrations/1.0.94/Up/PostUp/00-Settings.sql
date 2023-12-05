DO $$
BEGIN

-- Initialize settings.  These will need to be configured correctly for each environment manually.

INSERT INTO public."setting" (
  "name"
  , "description"
  , "value"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
)
VALUES (
  'AdvancedSearchMediaTypeGroups'
  , 'Provides a way to group related sources by their media type.'
  , '{ "newsRadioId": 4, "talkRadioId": 5, "televisionId": 6, "cpWireId": 7, "frontPageId": 11, "dailyPrintId": 1,   "weeklyPrintId": 2, "onlinePrintId": 3 }'
  , true
  , 0
  , ''
  , ''
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
)
VALUES (
  'AlertId'
  , 'The basic alert that informs subscribers of stories that Editor''s have "alerted".'
  , '1'
  , true
  , 0
  , ''
  , ''
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
)
VALUES (
  'DefaultSubscriberContentLicenseId'
  , 'Default license to use for content dynamically created for reports by subscribers.'
  , '1'
  , true
  , 0
  , ''
  , ''
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
)
VALUES (
  'DefaultSubscriberContentMediaTypeId'
  , 'Default media type to use for content dynamically created for reports by subscribers.'
  , '3'
  , true
  , 0
  , ''
  , ''
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
)
VALUES (
  'FrontpageFilterId'
  , 'Filter used for the subscriber "Today''s Front Pages" session'
  , '75'
  , true
  , 0
  , ''
  , ''
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
)
VALUES (
  'FrontPageImagesReportId'
  , 'Front Page Images report that is sent out each morning.'
  , '7'
  , true
  , 0
  , ''
  , ''
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
)
VALUES (
  'MorningReportId'
  , 'Morning report that is auto sent each morning.'
  , '13'
  , true
  , 0
  , ''
  , ''
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
)
VALUES (
  'NewsRadioMediaTypeFilterId'
  , 'The news radio media type ID used by the transcript page for filtering.'
  , '4'
  , true
  , 0
  , ''
  , ''
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
)
VALUES (
  'ProductSubscriptionManagerEmail'
  , 'The Email address to send Product Subscription requests to'
  , 'jeremy.1.foster@gov.bc.ca'
  , true
  , 0
  , ''
  , ''
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
)
VALUES (
  'SubscriberUrl'
  , 'URL to the subscriber site'
  , 'https://dev.mmi.gov.bc.ca/'
  , true
  , 0
  , ''
  , ''
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
)
VALUES (
  'TopStoryAlertId'
  , 'The Notification for top stories.  These are sent out each morning.'
  , '2'
  , true
  , 0
  , ''
  , ''
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
)
VALUES (
  'DefaultReportTemplateId'
  , 'The default report template for subscriber reports.'
  , '6'
  , true
  , 0
  , ''
  , ''
)
ON CONFLICT DO NOTHING;

END $$;
