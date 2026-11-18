DO $$
BEGIN

UPDATE public."tag" SET "is_enabled" = false WHERE "name" = '-- UPDATE MIGRATED TAG NAME --';

END $$;
