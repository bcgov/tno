-- Remove schema version 1 automation profiles: the page, engine, and API no longer support
-- them (their v2 copies were created before this migration). Idempotent - a database without
-- v1 profiles is a no-op.
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM information_schema.columns
             WHERE table_name = 'automation_profile' AND column_name = 'schema_version') THEN
    DELETE FROM automation_run
      WHERE automation_profile_id IN (SELECT id FROM automation_profile WHERE schema_version < 2);
    DELETE FROM schedule
      WHERE id IN (SELECT schedule_id FROM event_schedule
                   WHERE automation_profile_id IN (SELECT id FROM automation_profile WHERE schema_version < 2));
    DELETE FROM automation_profile WHERE schema_version < 2;
  END IF;
END $$;
