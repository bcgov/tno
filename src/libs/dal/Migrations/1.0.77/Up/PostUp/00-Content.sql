DO $$
BEGIN

UPDATE public.content SET summary = '' WHERE summary = '[TBD]';

UPDATE public.content SET headline = '' WHERE headline = '[TBD]';

END $$;
