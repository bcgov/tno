DO $$
BEGIN

ALTER TABLE public."source" ADD COLUMN "is_cbra_source" boolean NOT NULL DEFAULT false;
ALTER TABLE public."series" ADD COLUMN "is_cbra_source" boolean NOT NULL DEFAULT false;

END $$;
