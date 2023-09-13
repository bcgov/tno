DO $$
BEGIN

UPDATE "content"
SET "posted_on" = c."published_on"
FROM public."content" AS c;

END $$;
