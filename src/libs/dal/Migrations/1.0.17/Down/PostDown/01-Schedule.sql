DO $$
BEGIN

delete from public.schedule
where name in (
  'TNO 1.0 - AudioVideo Content', -- name
  'TNO 1.0 - Print Content', -- name
  'TNO 1.0 - Image Content', -- name
  'TNO 1.0 - Story Content' -- name
);

END $$;
