DO $$
BEGIN

UPDATE public.content SET summary = '' WHERE summary = '[TBD]'

END $$;
