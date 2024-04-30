DO $$
BEGIN

IF NOT EXISTS (SELECT * from public."user" WHERE username = 'contentmigrator') THEN
  INSERT INTO public."user" (username, email, "key", display_name, first_name, last_name, is_enabled, "status", email_verified, is_system_account, last_login_on, note, code, code_created_on, roles, created_by, created_on, updated_by, updated_on, "version", preferences, unique_logins, preferred_email) VALUES
	  ('contentmigrator', 'mmi+contentmigrator@gov.bc.ca', gen_random_uuid(), 'content migration service account', 'content migration', 'service user', true, 0, true, false, NULL, 'content migration service user', '', NULL, '[editor]', '', CURRENT_DATE, '', CURRENT_DATE, 1, '{}', 0, '');
END IF;

END $$;
