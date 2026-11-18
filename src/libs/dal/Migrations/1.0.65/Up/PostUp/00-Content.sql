DO $$
BEGIN

-- Unable to execute this query because the database in Openshift cannot handle the request.
-- It's unclear why this will not work.  The pod doesn't even try and grab more resources.
-- UPDATE "content"
-- SET "posted_on" = c."published_on"
-- FROM public."content" AS c
-- WHERE c."published_on" IS NOT NULL
--   AND c."status" > 0;

END $$;
