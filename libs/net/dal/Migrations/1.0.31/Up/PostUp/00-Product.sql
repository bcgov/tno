DO $$
BEGIN

UPDATE public."product"
SET "name" = 'Front Page Images'
WHERE "name" = 'Front Page';

END $$;
