DO $$
BEGIN

-- Update report template settings
UPDATE public."report_template" SET
  "settings" = '{
    "enableSummaryCharts": false,
    "enableSectionCharts": false
  }';

END $$;
