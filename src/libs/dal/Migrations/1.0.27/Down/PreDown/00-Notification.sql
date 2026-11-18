DO $$
BEGIN

DELETE FROM public."report_instance_content"
WHERE "SectionName" != '';

END $$;
