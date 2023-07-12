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
)
SELECT
  'main' -- name
  , '' -- description
  , true -- is_enabled
  , 0 -- sort_order
  , rf."report_id" -- report_id
  , (SELECT "id" FROM public."filter" WHERE "name" = rf."name" AND "owner_id" = rf."owner_id") -- filter_id
  , '{
    "label": "",
    "isSummary": false,
    "showContent": true,
    "showCharts": false,
    "chartsOnTop": true,
    "chartDirection": "row",
    "removeDuplicates": false,
    "sortBy": "alpha"
  }' -- settings
  , '' -- created_by
  , '' -- updated_by
FROM _report_filter rf;

DROP TABLE _report_filter;

END $$;
