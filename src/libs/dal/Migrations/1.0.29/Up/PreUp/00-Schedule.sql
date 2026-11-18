DO $$
BEGIN

ALTER TABLE public."schedule"
DROP COLUMN "repeat";

ALTER TABLE public."schedule"
ADD COLUMN "repeat" BOOL NOT NULL DEFAULT false;

END $$;
