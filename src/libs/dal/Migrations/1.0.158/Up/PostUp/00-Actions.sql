DO $$
BEGIN

UPDATE public."action"
SET "name" = 'Featured Story'
WHERE "name" = 'Homepage';

END $$;
