DO $$
BEGIN

-- name will get truncated if you rollback
ALTER TABLE "schedule" ALTER COLUMN "name" TYPE character varying(50)
USING substr("name", 1, 50)

END $$;
