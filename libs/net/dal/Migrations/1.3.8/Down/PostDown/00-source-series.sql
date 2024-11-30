DO $$
BEGIN

ALTER TABLE public."source" DROP COLUMN "is_cbra_source";
ALTER TABLE public."series" DROP COLUMN "is_cbra_source";

END $$;
