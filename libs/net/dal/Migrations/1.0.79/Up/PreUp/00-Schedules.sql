DO $$
BEGIN

DELETE FROM public."schedule"
WHERE "id" IN (
  SELECT "schedule_id"
  FROM public."folder"
);

END $$;
