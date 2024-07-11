DO $$
BEGIN

UPDATE public."user" SET
  "account_type" = 0 -- Direct user
WHERE "email" LIKE '%@gov.bc.ca'
  AND "is_system_account" = false;

UPDATE public."user" SET
  "account_type" = 1 -- Indirect user
WHERE "email" NOT LIKE '%@gov.bc.ca'
  AND "is_system_account" = false
  AND "email" != '';

UPDATE public."user" SET
  "account_type" = 2 -- Distribution
WHERE "email" NOT LIKE '%@gov.bc.ca'
  AND "is_system_account" = false
  AND "email" = '';

UPDATE public."user" SET
  "account_type" = 3 -- System Account
WHERE "is_system_account" = true;

END $$;
