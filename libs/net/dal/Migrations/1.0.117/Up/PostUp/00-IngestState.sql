DO $$
BEGIN

UPDATE public.ingest_state 
SET creation_date_of_last_item = (
	SELECT to_timestamp("configuration"->>'creationDateOfLastImport', 'YYYY-MM-DD HH12:MI:SS AM')
	FROM public.ingest
	WHERE public.ingest_state.ingest_id = public.ingest.id
	AND "configuration"->>'creationDateOfLastImport' IS NOT NULL
	);

UPDATE public.ingest 
SET "configuration" = "configuration" - 'creationDateOfLastImport'
WHERE "configuration"->>'creationDateOfLastImport' IS NOT NULL;

END $$;
