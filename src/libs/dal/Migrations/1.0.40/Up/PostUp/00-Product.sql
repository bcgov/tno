DO $$
BEGIN

UPDATE public.product SET name = 'Online' WHERE name = 'Online Print';

UPDATE public.product SET name = 'Events' WHERE name = 'Speeches & Scrums';

END $$;
