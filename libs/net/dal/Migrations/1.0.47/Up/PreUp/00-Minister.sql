DO $$
BEGIN

UPDATE public."minister"
SET "aliases" = ''
WHERE "aliases" IS NULL;

END $$;
