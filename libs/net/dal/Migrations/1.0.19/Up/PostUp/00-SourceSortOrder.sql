DO $$

BEGIN

UPDATE public.source SET sort_order = -2 WHERE name = 'Ming Pao News' AND sort_order != -2;
UPDATE public.source SET sort_order = -3 WHERE name = 'The Daily Courier (Kelowna)' AND sort_order != -3;
UPDATE public.source SET sort_order = -4 WHERE name = 'National Post' AND sort_order != -4;
UPDATE public.source SET sort_order = -5 WHERE name = 'Globe and Mail' AND sort_order != -5;
UPDATE public.source SET sort_order = -6 WHERE name = 'Times Colonist (Victoria)' AND sort_order != -6;
UPDATE public.source SET sort_order = -7 WHERE name = 'The Province' AND sort_order != -7;
UPDATE public.source SET sort_order = -8 WHERE name = 'Vancouver Sun' AND sort_order != -8;

END $$;
