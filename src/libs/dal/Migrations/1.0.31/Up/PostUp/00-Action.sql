DO $$
BEGIN

UPDATE public."action"
SET "name" = 'Homepage'
WHERE "name" = 'Just In';

DELETE FROM public."action"
WHERE "name" = 'On Ticker';

END $$;
