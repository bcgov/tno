DO $$
BEGIN

-- Extract the filters for the reports
CREATE TEMP TABLE _report_filter (
  "id"
  , "filter"
) AS
SELECT rs."report_id"
  , f."query"
FROM public."filter" f
INNER JOIN public."report_section" rs ON f."id" = rs."filter_id";

END $$;
