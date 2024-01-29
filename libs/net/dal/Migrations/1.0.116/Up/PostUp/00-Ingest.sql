DO $$
BEGIN

UPDATE public."ingest"
SET "reset_retry_after_delay_ms" = 3600
WHERE "reset_retry_after_delay_ms" = 0;

END $$;
