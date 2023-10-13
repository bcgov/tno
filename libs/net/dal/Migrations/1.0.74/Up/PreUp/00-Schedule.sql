DO $$
BEGIN

-- make the table longer.
ALTER TABLE "schedule" ALTER COLUMN "name" TYPE character varying(100) ;

END $$;
