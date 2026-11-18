UPDATE public."chart_template"
SET "settings" = "settings" - 'aggregate'
WHERE "name" = 'Custom';
