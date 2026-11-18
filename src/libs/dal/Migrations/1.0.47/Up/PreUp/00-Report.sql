DO $$
BEGIN

-- Extract the filters from the reports
CREATE TEMP TABLE _report_filter (
  "report_id"
  , "name"
  , "query"
  , "owner_id"
) AS
SELECT "id"
  , "name"
  , "filter"
  , "owner_id"
FROM public."report";

END $$;
