DO $$
BEGIN

UPDATE public.content SET summary = '' WHERE summary IN ('[TBD]', '<p>TBD</p>');

UPDATE public.content SET headline = '' WHERE headline IN ('[TBD]', '<p>TBD</p>');

END $$;
