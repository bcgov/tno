DO $$
BEGIN

-- this value was incorrectly stored in seconds from the UI layer previously
-- assume anything less than 10000 is actualy in seconds and convert it to milliseconds
UPDATE public."ingest"
SET reset_retry_after_delay_ms = (reset_retry_after_delay_ms * 1000)
WHERE reset_retry_after_delay_ms <> 0 AND reset_retry_after_delay_ms < 10000;

END $$;
