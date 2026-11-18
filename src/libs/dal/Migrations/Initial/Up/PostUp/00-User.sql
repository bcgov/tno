DO $$
BEGIN

INSERT INTO public.user (
  "username"
  , "key"
  , "email"
  , "display_name"
  , "email_verified"
  , "is_enabled"
  , "is_system_account"
  , "created_by"
  , "updated_by"
) VALUES (
  'service-account' -- username
  , '616beebf-ce6c-4b28-bd5f-a32ceded524b' -- key
  , 'service-account@local.com' -- email
  , 'Service Account' -- displayName
  , true -- emailVerified
  , true -- isEnabled
  , true -- isServiceAccount
  , ''
  , ''
);

END $$;
