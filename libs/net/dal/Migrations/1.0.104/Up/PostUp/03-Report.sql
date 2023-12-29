DO $$
BEGIN

-- Insert new report
INSERT INTO public."report" (
  "name"
  , "description"
  , "owner_id"
  , "report_template_id"
  , "settings"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'Event of the Day' -- name
  , '' -- description
  , 1 -- owner_id
  , (select id from public."report_template" where "name"  = 'Event of the Day' limit 1) -- report_template_id
  , '{
  "content": {
    "clearFolders": false,
    "excludeReports": [],
    "onlyNewContent": false,
    "showLinkToStory": false,
    "excludeHistorical": false,
    "highlightKeywords": false
  },
  "subject": {
    "text": "Event of the Day",
    "showTodaysDate": true
  },
  "headline": {
    "showByline": false,
    "showSource": false,
    "showSentiment": false,
    "showShortName": false,
    "showPublishedOn": false
  },
  "sections": {
    "usePageBreaks": false
  }
}' --settings
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
);

END $$;
