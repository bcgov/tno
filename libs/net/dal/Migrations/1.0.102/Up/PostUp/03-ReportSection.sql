DO $$
BEGIN

-- Create report sections.
INSERT INTO public."report_section" (
  "name"
  , "description"
  , "is_enabled"
  , "sort_order"
  , "report_id"
  , "filter_id"
  , "settings"
  , "created_by"
  , "updated_by"
) VALUES (
  '6ed85762-3634-4c6b-880d-77b88c3afac0' -- name
  , '' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , (SELECT "id" FROM public."report" WHERE "name" = 'Event of the Day')  -- report_id
  , (SELECT "id" FROM public."filter" WHERE "name" = 'Event of the Day - 24hr Aggregate') -- filter_id
  , '{
  "label": "Top Topics - Last 24hrs",
  "sortBy": "",
  "groupBy": "",
  "direction": "",
  "hideEmpty": false,
  "showImage": false,
  "sectionType": "MediaAnalytics",
  "showFullStory": false,
  "showHeadlines": false,
  "useAllContent": false,
  "removeDuplicates": false
}' -- settings
  , '' -- created_by
  , '' -- updated_by
),(
  'b5140713-c6d5-4be5-9318-94925876ca1c' -- name
  , '' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , (SELECT "id" FROM public."report" WHERE "name" = 'Event of the Day')  -- report_id
  , (SELECT "id" FROM public."filter" WHERE "name" = 'Event of the Day - 24hr Aggregate') -- filter_id
  , '{
  "label": "",
  "sortBy": "",
  "groupBy": "",
  "direction": "column",
  "hideEmpty": false,
  "showImage": false,
  "sectionType": "Content",
  "showFullStory": false,
  "showHeadlines": false,
  "useAllContent": false,
  "removeDuplicates": false
}' -- settings
  , '' -- created_by
  , '' -- updated_by
),
(
  'fb499f42-6437-4fc2-9bd6-05918cf52539' -- name
  , '' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , (SELECT "id" FROM public."report" WHERE "name" = 'Event of the Day')  -- report_id
  , (SELECT "id" FROM public."filter" WHERE "name" = 'Event of the Day - Rolling 365 day aggregate') -- filter_id
  , '{
  "label": "Top Topics - Last 356 days",
  "sortBy": "",
  "groupBy": "",
  "direction": "",
  "hideEmpty": false,
  "showImage": false,
  "sectionType": "Content",
  "showFullStory": false,
  "showHeadlines": false,
  "useAllContent": false,
  "removeDuplicates": false
}' -- settings
  , '' -- created_by
  , '' -- updated_by
);

END $$;
