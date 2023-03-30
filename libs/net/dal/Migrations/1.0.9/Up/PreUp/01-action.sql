DO $$
BEGIN

DELETE FROM public."action"
WHERE name = 'Front Page';

END $$;
