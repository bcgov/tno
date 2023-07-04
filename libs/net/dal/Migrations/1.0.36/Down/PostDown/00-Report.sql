DO $$
BEGIN

UPDATE public."report"
SET "template" = r."body"
FROM _report_template r
WHERE "report".id = r.id;

DROP TABLE _report_template;

END $$;
