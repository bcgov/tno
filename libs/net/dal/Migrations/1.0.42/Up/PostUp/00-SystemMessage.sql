DO $$
BEGIN

IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'alert') THEN
    ALTER TABLE alert RENAME TO system_message;

    IF EXISTS (
	    SELECT 1 FROM pg_constraint
	    WHERE conrelid = (SELECT oid FROM pg_class WHERE relname = 'system_message')
	    AND conname = 'PK_alert') THEN
        ALTER TABLE system_message DROP CONSTRAINT "PK_alert";
    END IF;

    ALTER TABLE system_message ADD CONSTRAINT PK_system_message PRIMARY KEY (id);
END IF;

END $$;
