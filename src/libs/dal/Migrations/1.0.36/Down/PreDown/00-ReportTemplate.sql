DO $$
BEGIN

-- Grab all the templates for the reports.
CREATE TEMP TABLE _report_template (
  "id"
  , "subject"
  , "body"
) AS
SELECT r."id"
  , rt."subject"
  , rt."body"
FROM public."report_template" rt
JOIN public."report" r ON r."report_template_id" = rt."id";

END $$;
