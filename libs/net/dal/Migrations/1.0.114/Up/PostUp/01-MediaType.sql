DO $$
BEGIN

Update public.media_type
set sort_order = 10
where "name" = 'Weekly Print';

Update public.media_type
set sort_order = 20
where "name" = 'Daily Print';

Update public.media_type
set sort_order = 30
where "name" = 'Talk Radio';

Update public.media_type
set sort_order = 40
where "name" = 'TV / Video News';

Update public.media_type
set sort_order = 50
where "name" = 'Online';

Update public.media_type
set sort_order = 60
where "name" = 'CP Wire';

Update public.media_type
set sort_order = 70
where "name" = 'News Radio';

END $$;
