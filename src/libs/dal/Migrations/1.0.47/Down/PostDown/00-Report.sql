DO $$
BEGIN

-- Update report settings
UPDATE public."report" r
SET "filter" = rf."filter"
  , "report_type" = 1
FROM (
  SELECT * FROM _report_filter
) AS rf
WHERE r."id" = rf."id";

DROP TABLE _report_filter;

END $$;
