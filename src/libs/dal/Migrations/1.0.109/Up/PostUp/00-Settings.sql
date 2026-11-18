DO $$
DECLARE ReportId INT;
BEGIN

-- get the Report Id
SELECT INTO ReportId id FROM public."report" where "name" = 'Event of the Day' and owner_id = 1;

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
  'EventOfTheDayReportId'
  , 'Event of the Day report ID'
  , ReportId
  , true
  , 0
  , ''
  , ''
)
ON CONFLICT DO NOTHING;

END $$;
