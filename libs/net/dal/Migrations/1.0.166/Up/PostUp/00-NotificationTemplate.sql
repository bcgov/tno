DO $$
BEGIN

-- Update name field in report template to match all environments:
UPDATE public."report_template" 
SET "name" = 'Evening Overview - Weekday'
WHERE "name" = 'AV Overview - Weekday';

UPDATE public."report_template"
SET "name" = 'Evening Overview - Weekend'
WHERE "name" = 'AV Overview - Weekend';

END $$;