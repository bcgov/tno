DO $$
BEGIN

DELETE FROM public."schedule"
WHERE "id" = (SELECT id FROM public."schedule" WHERE "name" = 'Morning Report' ORDER BY "id" DESC LIMIT 1);

ALTER TABLE public."schedule"
DROP COLUMN "repeat";

ALTER TABLE public."schedule"
ADD COLUMN "repeat" INT4 NOT NULL DEFAULT 0;

END $$;
