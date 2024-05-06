DO $$
BEGIN

IF NOT EXISTS (SELECT * from public."user" WHERE username = 'contentmigrator') THEN
  INSERT INTO public."user" (
    "username"
    , "email"
    , "key"
    , "display_name"
    , "first_name"
    , "last_name"
    , "is_enabled"
    , "status"
    , "email_verified"
    , "is_system_account"
    , "note"
    , "roles"
    , "preferences"
    , "preferred_email"
    , "created_by"
    , "created_on"
    , "updated_by"
    , "updated_on"
  ) VALUES (
    'contentmigrator' -- username
    , 'mmi+contentmigrator@gov.bc.ca' -- email
    , gen_random_uuid() -- key
    , 'content migration service account' -- display_name
    , 'content migration' -- first_name
    , 'service user' -- last_name
    , true -- is_enabled
    , 2 -- status
    , true -- email_verified
    , false -- is_system_account
    , 'TNO content effort time tracking account.  Unable to map to actual users who assigned the effort to the story originally.  This is for the CBRA report.' -- note
    , '[editor]' -- roles
    , '{}' -- preferences
    , '' -- preferred_email
    , '' -- created_by
    , CURRENT_DATE -- created_on
    , '' -- updated_by
    , CURRENT_DATE -- updated_on
  );
END IF;

END $$;
